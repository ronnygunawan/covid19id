import { CombinedStatistics } from "../models/CombinedStatistics";

const realtimeUrl = "https://services1.arcgis.com/0MSEUqKaxRlEPj5g/arcgis/rest/services/ncov_cases/FeatureServer/1/query?f=json&where=Confirmed%20%3E%200&returnGeometry=false&spatialRel=esriSpatialRelIntersects&outFields=*&orderByFields=Confirmed%20desc%2CCountry_Region%20asc%2CProvince_State%20asc&resultOffset=0&resultRecordCount=250&cacheHint=true";
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

async function getRealtimeStatistics(): Promise<RealtimeApiDailyStatistics[]> {
    const response = await fetch(realtimeUrl);
    const result: RealtimeApiResult = await response.json();
    return result.features.map(feature => feature.attributes);
}

function parseCsv(csv: string): CsvModel[] {
    const [header, ...lines] = csv.split("\n");
    const [,,,,...dates] = header.split(",");
    const records: CsvModel[] = [];
    for (const line of lines) {
        const [province_state, country_region, lat, long, ...counts] = line.split(",");
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
        records.push(record);
    }
    return records;
}

async function getHistoricalStatistics(): Promise<CombinedStatistics[]> {
    const confirmedCasesResponse = await fetch(confirmedCasesUrl);
    const confirmedCases = parseCsv(await confirmedCasesResponse.text());
    const deathCasesResponse = await fetch(deathCasesUrl);
    const deathCases = parseCsv(await deathCasesResponse.text());
    const recoveredCasesResponse = await fetch(recoveredCasesUrl);
    const recoveredCases = parseCsv(await recoveredCasesResponse.text());
    return confirmedCases.map((cc): CombinedStatistics => ({
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

export async function getCountries(): Promise<string[]> {
    if (allStatistics === null) {
        allStatistics = await getAllStatistics();
    }
    const countries = Object.keys(allStatistics.reduce<{
        [country: string]: 1
    }>((prev, cur) => {
        prev[cur.Country_Region] = 1;
        return prev;
    }, {}));
    countries.sort((a, b) => a.localeCompare(b));
    return countries;
}

export async function getProvinces(country: string): Promise<string[]> {
    if (allStatistics === null) {
        allStatistics = await getAllStatistics();
    }
    const provinces = Object.keys(allStatistics.filter(stat => stat.Country_Region === country && stat.Province_State).reduce<{
        [province: string]: 1
    }>((prev, cur) => {
        prev[cur.Province_State!] = 1;
        return prev;
    }, {}));
    provinces.sort((a, b) => a.localeCompare(b));
    return provinces;
}

export async function getStatistics(province_state: string | null, country_region: string): Promise<CombinedStatistics | null> {
    if (allStatistics === null) {
        allStatistics = await getAllStatistics();
    }
    if (province_state === null) {
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
            return statistics[0];
        }
    } else {
        return allStatistics.find(stat => stat.Province_State === province_state && stat.Country_Region === country_region) || null;
    }
}