import { CombinedStatistics } from "../models/CombinedStatistics";
import * as Papa from "papaparse";
import { USStates } from "../helpers/USStates";

const realtimeUrl = "https://services1.arcgis.com/0MSEUqKaxRlEPj5g/arcgis/rest/services/ncov_cases/FeatureServer/1/query?f=json&where=Confirmed%20%3E%200&returnGeometry=false&spatialRel=esriSpatialRelIntersects&outFields=*&orderByFields=Confirmed%20desc&outSR=102100&resultOffset=0&resultRecordCount=250&cacheHint=true";
const confirmedCasesUrl = "https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_19-covid-Confirmed.csv";
const deathCasesUrl = "https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_19-covid-Deaths.csv";
const recoveredCasesUrl = "https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_19-covid-Recovered.csv";

interface RealtimeApiDailyStatistics {
    Province_State: string | null;
    Country_Region: string;
    Last_Update: number;
    Lat: number;
    Long_: number;
    Confirmed: number;
    Deaths: number;
    Recovered: number;
}

interface RealtimeApiResult {
    features: {
        attributes: RealtimeApiDailyStatistics
    }[]
}

interface CsvModel {
    Province_State: string;
    Country_Region: string;
    Lat: number;
    Long: number;
    TimeSeries: {
        Date: string;
        Cases: number;
    }[];
}

const normalizeName = (name: string): string => {
    if (!name) {
        return name;
    } else if (name === '"Diamond Princess" cruise ship'
        || name === "Diamond Princess cruise ship"
        || name === "From Diamond Princess"
        || name.endsWith("(From Diamond Princess)")) {
        return "Diamond Princess";
    } else if (name === "Mainland China") {
        return "China";
    } else if (name === "Democratic Republic of the Congo"
        || name === "Congo (Kinshasa)") {
        return "Congo";
    } else if (name === "Republic of Korea"
        || name === "Korea, South") {
        return "South Korea"
    } else if (name === "Hong Kong SAR") {
        return "Hong Kong";
    } else if (name === "Iran (Islamic Republic of)") {
        return "Iran";
    } else if (name === "occupied Palestinian territory") {
        return "Palestine";
    } else if (name === "Macao SAR") {
        return "Macau";
    } else if (name === "Republic of Moldova") {
        return "Moldova";
    } else if (name === "Russian Federation") {
        return "Russia";
    } else if (name === "Saint Martin") {
        return "St. Martin";
    } else if (name === "Taipei and environs"
        || name === "Taiwan*") {
        return "Taiwan";
    } else if (name === "Viet Nam") {
        return "Vietnam";
    } else {
        return name;
    }
}

export async function getRealtimeStatistics(): Promise<RealtimeApiDailyStatistics[]> {
    const response = await fetch(realtimeUrl);
    const result: RealtimeApiResult = await response.json();
    return result.features.reduce((agg: RealtimeApiDailyStatistics[], feature) => {
        const stat = feature.attributes;
        const dailyStat = {
            ...stat,
            Province_State: stat.Province_State ? normalizeName(stat.Province_State) : null,
            Country_Region: normalizeName(stat.Country_Region)
        };
        switch (dailyStat.Country_Region) {
            case "China":
                switch (dailyStat.Province_State) {
                    case "Hong Kong":
                    case "Macau":
                    case "Taiwan":
                        agg.push({
                            ...dailyStat,
                            Country_Region: dailyStat.Province_State,
                            Province_State: null
                        });
                        break;
                    default:
                        agg.push(dailyStat);
                        break;
                }
                break;
            case "Hong Kong":
            case "Macau":
            case "Taiwan":
                agg.push({
                    ...dailyStat,
                    Province_State: null
                });
                break;
            default:
                agg.push(dailyStat);
                break;
        }
        return agg;
    }, []);
}

function parseCsv(csv: string): CsvModel[] {
    const csvData: Papa.ParseResult = Papa.parse(csv, {
        header: false
    });
    const [header, ...csvRows]: string[][] = csvData.data;
    const [, , , , ...dates]: string[] = header;
    const records: CsvModel[] = [];
    for (const csvRow of csvRows) {
        const [city_province_state, country, lat, long, ...counts] = csvRow;
        let country_region = country;
        let province_state = city_province_state;
        if (country_region === "US") {
            if (city_province_state === "Grand Princess Cruise Ship") {
                province_state = "Grand Princess";
            } else if (city_province_state.endsWith("(From Diamond Princess)")) {
                province_state = "Diamond Princess";
            } else {
                const [city, state_code] = city_province_state.split(", ");
                province_state = state_code ? USStates[state_code] : city;
            }
        }
        province_state = normalizeName(province_state);
        country_region = normalizeName(country_region);
        const record: CsvModel = {
            Province_State: province_state,
            Country_Region: country_region,
            Lat: parseFloat(lat),
            Long: parseFloat(long),
            TimeSeries: dates.map((date, i) => {
                const cases = parseInt(counts[i]) || 0;
                return {
                    Date: date,
                    Cases: cases
                };
            })
        };
        switch (record.Country_Region) {
            case "China":
                switch (record.Province_State) {
                    case "Hong Kong":
                    case "Macau":
                    case "Taiwan":
                        records.push({
                            ...record,
                            Country_Region: record.Province_State,
                            Province_State: ""
                        });
                        break;
                    default:
                        records.push(record);
                        break;
                }
                break;
            case "Hong Kong":
            case "Macau":
            case "Taiwan":
                records.push({
                    ...record,
                    Province_State: ""
                });
                break;
            default:
                records.push(record);
                break;
        }
    }
    return records;
}

export async function getHistoricalStatistics(): Promise<CombinedStatistics[]> {
    const confirmedCasesResponse = await fetch(confirmedCasesUrl);
    const confirmedCases = parseCsv(await confirmedCasesResponse.text());
    const deathCasesResponse = await fetch(deathCasesUrl);
    const deathCases = parseCsv(await deathCasesResponse.text());
    const recoveredCasesResponse = await fetch(recoveredCasesUrl);
    const recoveredCases = parseCsv(await recoveredCasesResponse.text());
    const result = confirmedCases.map((cc): CombinedStatistics => ({
        ...cc,
        Province_State: cc.Province_State === "" ? null : cc.Province_State,
        TimeSeries: cc.TimeSeries.map(c => {
            const confirmed = c.Cases;
            const deaths = deathCases
                .find(dc => dc.Province_State === cc.Province_State && dc.Country_Region === cc.Country_Region)!
                ?.TimeSeries.find(d => d.Date === c.Date)
                ?.Cases;
            if (deaths === undefined) throw new Error();
            const recovered = recoveredCases
                .find(rc => rc.Province_State === cc.Province_State && rc.Country_Region === cc.Country_Region)!
                ?.TimeSeries.find(r => r.Date === c.Date)
                ?.Cases;
            if (recovered === undefined) throw new Error();
            return {
                Date: c.Date,
                Confirmed: confirmed,
                Deaths: deaths,
                Recovered: recovered
            };
        })
    }));
    // trim end
    for (const provinceResult of result) {
        if (provinceResult.TimeSeries.length > 2) {
            const last = provinceResult.TimeSeries[provinceResult.TimeSeries.length - 1];
            const beforeLast = provinceResult.TimeSeries[provinceResult.TimeSeries.length - 2];
            if (last.Confirmed === 0
                && last.Deaths === 0
                && last.Recovered === 0
                && (beforeLast.Confirmed !== 0
                    || beforeLast.Deaths !== 0
                    || beforeLast.Recovered !== 0)) {
                for (const provinceResult of result) {
                    if (provinceResult.TimeSeries.length > 1) {
                        provinceResult.TimeSeries.pop();
                    }
                }
                break;
            }
        }
    }
    return result;
}

async function getAllStatistics(): Promise<CombinedStatistics[]> {
    const historicalStatistics = await getHistoricalStatistics();
    const realtimeStatistics = await getRealtimeStatistics();
    const allStatistics: CombinedStatistics[] = realtimeStatistics.map((rs): CombinedStatistics => {
        const hs: CombinedStatistics = historicalStatistics.find(hs => hs.Province_State === rs.Province_State && hs.Country_Region === rs.Country_Region) || {
            Province_State: rs.Province_State,
            Country_Region: rs.Country_Region,
            Lat: rs.Lat,
            Long: rs.Long_,
            TimeSeries: []
        };
        return {
            ...hs,
            TimeSeries: [
                ...hs.TimeSeries,
                {
                    Date: "TODAY",
                    Confirmed: rs.Confirmed,
                    Deaths: rs.Deaths,
                    Recovered: rs.Recovered
                }
            ]
        };
    });
    return allStatistics;
}

var allStatistics: CombinedStatistics[] | null = null;

export async function loadStatistics(): Promise<void> {
    allStatistics = await getAllStatistics();
}

export async function getCountries(): Promise<[string, number][]> {
    if (allStatistics === null) {
        allStatistics = await getAllStatistics();
    }
    const confirmedByCountry = allStatistics.reduce<{
        [country: string]: number
    }>((prev, cur) => {
        if (prev[cur.Country_Region]) {
            prev[cur.Country_Region] += cur.TimeSeries[cur.TimeSeries.length - 1].Confirmed;
        } else {
            prev[cur.Country_Region] = cur.TimeSeries[cur.TimeSeries.length - 1].Confirmed;
        }
        return prev;
    }, {});
    const countries = Object.keys(confirmedByCountry);
    countries.sort((a, b) => a.localeCompare(b));
    return countries.map(country => [country, confirmedByCountry[country]]);
}

export async function getProvinces(country: string): Promise<[string, number][]> {
    if (allStatistics === null) {
        allStatistics = await getAllStatistics();
    }
    const confirmedByProvince = allStatistics.filter(stat => stat.Country_Region === country && stat.Province_State).reduce<{
        [province: string]: number
    }>((prev, cur) => {
        prev[cur.Province_State!] = cur.TimeSeries[cur.TimeSeries.length - 1].Confirmed;
        return prev;
    }, {})
    const provinces = Object.keys(confirmedByProvince);
    provinces.sort((a, b) => a.localeCompare(b));
    return provinces.map(province => [province, confirmedByProvince[province]]);
}

export async function getStatistics(province_state: string | null, country_region: string | null): Promise<CombinedStatistics | null> {
    if (allStatistics === null) {
        allStatistics = await getAllStatistics();
    }
    if (country_region === null) {
        if (allStatistics.length > 0) {
            return allStatistics.reduce<CombinedStatistics>((prev, cur) => {
                for (const ct of cur.TimeSeries) {
                    const pt = prev.TimeSeries.find(pt => pt.Date === ct.Date);
                    if (pt !== undefined) {
                        pt.Confirmed += ct.Confirmed;
                        pt.Deaths += ct.Deaths;
                        pt.Recovered += ct.Recovered;
                    }
                }
                return prev;
            }, {
                Country_Region: "SELURUH DUNIA",
                Lat: 0,
                Long: 0,
                Province_State: null,
                TimeSeries: allStatistics[0].TimeSeries.map(t => ({
                    Date: t.Date,
                    Confirmed: 0,
                    Deaths: 0,
                    Recovered: 0
                }))
            });
        } else {
            return null;
        }
    } else if (country_region === "DI LUAR CHINA") {
        if (allStatistics.length > 0) {
            return allStatistics.filter(s => s.Country_Region !== "China").reduce<CombinedStatistics>((prev, cur) => {
                for (const ct of cur.TimeSeries) {
                    const pt = prev.TimeSeries.find(pt => pt.Date === ct.Date);
                    if (pt !== undefined) {
                        pt.Confirmed += ct.Confirmed;
                        pt.Deaths += ct.Deaths;
                        pt.Recovered += ct.Recovered;
                    }
                }
                return prev;
            }, {
                Country_Region: "DI LUAR CHINA",
                Lat: 0,
                Long: 0,
                Province_State: null,
                TimeSeries: allStatistics[0].TimeSeries.map(t => ({
                    Date: t.Date,
                    Confirmed: 0,
                    Deaths: 0,
                    Recovered: 0
                }))
            });
        } else {
            return null;
        }
    } else if (province_state === null) {
        const statistics = allStatistics.filter(stat => stat.Country_Region === country_region);
        if (statistics.length > 1) {
            return statistics.reduce<CombinedStatistics>((prev, cur) => {
                for (const ct of cur.TimeSeries) {
                    const pt = prev.TimeSeries.find(pt => pt.Date === ct.Date);
                    if (pt !== undefined) {
                        pt.Confirmed += ct.Confirmed;
                        pt.Deaths += ct.Deaths;
                        pt.Recovered += ct.Recovered;
                    }
                }
                return prev;
            }, {
                ...statistics[0],
                TimeSeries: statistics[0].TimeSeries.map(t => ({
                    Date: t.Date,
                    Confirmed: 0,
                    Deaths: 0,
                    Recovered: 0
                }))
            });
        } else {
            return statistics[0] || null;
        }
    } else {
        return allStatistics.find(stat => stat.Province_State === province_state && stat.Country_Region === country_region) || null;
    }
}