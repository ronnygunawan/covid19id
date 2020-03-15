import { CombinedStatistics } from "../models/CombinedStatistics";

export const toCFR1 = (statistics: CombinedStatistics, cfrDelay: number) => {
    const firstNonZero = statistics.TimeSeries.findIndex(t => t.Confirmed > 0);
    const cfr = [];
    if (firstNonZero !== -1
        && firstNonZero + cfrDelay < statistics.TimeSeries.length) {
        for (let i = firstNonZero + cfrDelay; i < statistics.TimeSeries.length; i++) {
            const deaths = statistics.TimeSeries[i].Deaths;
            const confirmed = statistics.TimeSeries[i - cfrDelay].Confirmed;
            if (confirmed > 0) {
                const rate = deaths / confirmed * 100;
                cfr.push({
                    x: statistics.TimeSeries[i].Date,
                    y: rate
                });
            }
        }
    }
    return [
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
        },
        {
            id: `CFR1(${cfrDelay})`,
            data: cfr
        }
    ];
};