import { CombinedStatistics } from "../models/CombinedStatistics";

export const toActiveCases = (statistics: CombinedStatistics) => [
    {
        id: "Belum Sembuh",
        data: statistics.TimeSeries.map(t => ({
            x: t.Date,
            y: t.Confirmed - t.Deaths - t.Recovered
        }))
    },
    {
        id: "Meninggal",
        data: statistics.TimeSeries.map(t => ({
            x: t.Date,
            y: t.Deaths
        }))
    },
    {
        id: "Sembuh",
        data: statistics.TimeSeries.map(t => ({
            x: t.Date,
            y: t.Recovered
        }))
    }
];