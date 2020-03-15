import { CombinedStatistics } from "../models/CombinedStatistics";
import { View } from "../components/ViewSelector";

const toIndonesiaCaseFatalityRate = (statistics: CombinedStatistics) => {
    const result: {
        id: string,
        data: {
            x: string,
            y: number | null
        }[]
    }[] = [
            {
                id: "CFR",
                data: []
            }
        ];
    let started = false;
    for (const t of statistics.TimeSeries) {
        if (t.Date === "3/1/20") {
            started = true;
        }
        if (started) {
            result[0].data.push({
                x: t.Date,
                y: t.Confirmed > 0 ? t.Deaths / t.Confirmed * 100 : 0
            });
        }
    }
    const [m, d, y] = statistics.TimeSeries[statistics.TimeSeries.length - 2].Date.split("/");
    const yesterday = new Date(parseInt(y) + 2000, parseInt(m) - 1, parseInt(d));
    const oneDay = 24 * 60 * 60 * 1000;
    const tomorrow = new Date(yesterday.getTime() + oneDay * 2);
    const maxTime = new Date(2020, 4, 31).getTime();
    for (let time = tomorrow.getTime(); time <= maxTime; time += oneDay) {
        const d = new Date(time);
        const date = `${d.getMonth() + 1}/${d.getDate()}/${d.getFullYear() - 2000}`;
        result[0].data.push({
            x: date,
            y: null
        });
    }
    return result;
};

export const toCaseFatalityRate = (statistics: CombinedStatistics, view: View) => statistics.Country_Region === "Indonesia" && view === "mudik"
    ? toIndonesiaCaseFatalityRate(statistics)
    : [
        {
            id: "CFR",
            data: statistics.TimeSeries.map(t => ({
                x: t.Date,
                y: t.Confirmed > 0 ? t.Deaths / t.Confirmed * 100 : 0
            }))
        }
    ];