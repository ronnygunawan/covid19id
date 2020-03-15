import { CombinedStatistics } from "../models/CombinedStatistics";
import { KawalCovid19idDailyStatistics } from "../models/KawalCovid19idDailyStatistics";

const summaryUrl = "https://kawalcovid19.harippe.id/api/summary";
const dailyStatisticsUrl = "https://covid19id.azurewebsites.net/kawalcovid19/statistik-harian";

interface KawalCovid19idSummary {
    confirmed: {
        value: number;
    };
    recovered: {
        value: number;
    };
    deaths: {
        value: number;
    };
    activeCare: {
        value: number;
    };
    metadata: {
        lastUpdatedAt: string;
    };
    nationality: { };
    cluster: { };
    province: { };
    gender: { };
}

var statistics: CombinedStatistics | null = null;

export async function getStatistics(): Promise<CombinedStatistics | null> {
    if (statistics === null) {
        const kawalcovid19idResponse = await fetch(summaryUrl);
        const result: KawalCovid19idSummary = await kawalcovid19idResponse.json();
        statistics = {
            Country_Region: "Indonesia",
            Province_State: null,
            Lat: -0.7893,
            Long: 113.9213,
            TimeSeries: [
                {
                    Date: "TODAY",
                    Confirmed: result.confirmed.value,
                    Deaths: result.deaths.value,
                    Recovered: result.recovered.value,
                    Observed: null,
                    Negatives: null
                }
            ]
        };
    }
    return statistics;
}

var dailyStatistics: KawalCovid19idDailyStatistics[] | null = null;

export async function getDailyStatistics(): Promise<KawalCovid19idDailyStatistics[] | null> {
    if (dailyStatistics === null) {
        const kawalcovid19idResponse = await fetch(dailyStatisticsUrl);
        const result: KawalCovid19idDailyStatistics[] = await kawalcovid19idResponse.json();
        dailyStatistics = result;
    }
    return dailyStatistics;
}