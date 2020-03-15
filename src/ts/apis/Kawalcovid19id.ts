import { CombinedStatistics } from "../models/CombinedStatistics";

const summaryUrl = "https://kawalcovid19.harippe.id/api/summary";

interface Kawalcovid19idSummary {
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
        const result: Kawalcovid19idSummary = await kawalcovid19idResponse.json();
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
                    Recovered: result.recovered.value
                }
            ]
        };
    }
    return statistics;
}