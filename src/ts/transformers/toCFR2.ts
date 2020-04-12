import { CombinedStatistics } from "../models/CombinedStatistics";

export const toCFR2 = (statistics: CombinedStatistics, cfrDelay: number) => {
    const firstNonZero = statistics.TimeSeries.findIndex(t => t.Confirmed > 0);
    const cfr = [];
    if (firstNonZero !== -1
        && firstNonZero + cfrDelay < statistics.TimeSeries.length) {
        for (let i = firstNonZero + cfrDelay; i < statistics.TimeSeries.length; i++) {
            const deaths = statistics.TimeSeries[i].Deaths;
            const recovered = statistics.TimeSeries[i - cfrDelay].Recovered;
            const total = deaths + (recovered || 0);
            if (total > 0) {
                const rate = deaths / total * 100;
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
            id: `CFR2(${cfrDelay})`,
            data: cfr
        }
    ];
};