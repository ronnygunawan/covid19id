import { CombinedStatistics } from "../models/CombinedStatistics";

export const toCumulativeCases = (statistics: CombinedStatistics) => [
    {
        id: "Positif",
        data: statistics.TimeSeries.map(t => ({
            x: t.Date,
            y: t.Confirmed
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