import { Datum } from "@nivo/line";
import { CombinedStatistics } from "../models/CombinedStatistics";
import { View } from "../components/ViewSelector";

interface DailyStatistics {
    Date: string;
    Confirmed: number;
    Deaths: number;
    Recovered: number | null;
    Negatives: number | null;
    Observed: number | null;
    PDP: number | null;
    ODP: number | null;
}

const mapDelta = (timeSeries: DailyStatistics[], selector: (x: DailyStatistics) => number | null): Datum[] => timeSeries.reduce<[Datum[], DailyStatistics | null]>((agg, cur) => {
    const [arr, prev] = agg;
    const y = selector(cur);
    if (y !== null) {
        if (prev === null) {
            arr.push({
                x: cur.Date,
                y: selector(cur)
            });
        } else {
            const yPrev = selector(prev) || 0;
            arr.push({
                x: cur.Date,
                y: y - yPrev
            });
        }
    }
    return [arr, cur];
}, [[], null])[0];

const toIndonesiaDailyObservations = (statistics: CombinedStatistics) => {
    const result: {
        id: string,
        data: Datum[]
    }[] = [
            {
                id: "Positif",
                data: mapDelta(statistics.TimeSeries, d => d.Confirmed)
            },
            {
                id: "Meninggal",
                data: mapDelta(statistics.TimeSeries, d => d.Deaths)
            },
            {
                id: "Sembuh",
                data: mapDelta(statistics.TimeSeries, d => d.Recovered)
            },
            {
                id: "Negatif",
                data: mapDelta(statistics.TimeSeries, d => d.Negatives)
            },
            {
                id: "Diperiksa",
                data: mapDelta(statistics.TimeSeries, d => d.Observed)
            },
            {
                id: "PDP baru",
                data: mapDelta(statistics.TimeSeries, d => d.PDP).slice(1)
            },
            {
                id: "ODP baru",
                data: mapDelta(statistics.TimeSeries, d => d.ODP).slice(1)
            }
        ];
    const indexOfMarch1st = statistics.TimeSeries.findIndex(t => t.Date === "3/1/20");
    if (indexOfMarch1st !== -1) {
        result[0].data.splice(0, indexOfMarch1st);
        result[1].data.splice(0, indexOfMarch1st);
        result[2].data.splice(0, indexOfMarch1st);
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
        result[1].data.push({
            x: date,
            y: null
        });
        result[2].data.push({
            x: date,
            y: null
        });
    }
    return result;
};

export const toDailyObservations = (statistics: CombinedStatistics, view: View) => statistics.Country_Region === "Indonesia" && view === "mudik"
    ? toIndonesiaDailyObservations(statistics)
    : [
        {
            id: "Positif",
            data: mapDelta(statistics.TimeSeries, d => d.Confirmed)
        },
        {
            id: "Meninggal",
            data: mapDelta(statistics.TimeSeries, d => d.Deaths)
        },
        {
            id: "Sembuh",
            data: mapDelta(statistics.TimeSeries, d => d.Recovered)
        },
        {
            id: "Negatif",
            data: mapDelta(statistics.TimeSeries, d => d.Negatives)
        },
        {
            id: "Diperiksa",
            data: mapDelta(statistics.TimeSeries, d => d.Observed)
        },
        {
            id: "PDP baru",
            data: mapDelta(statistics.TimeSeries, d => d.PDP).slice(1)
        },
        {
            id: "ODP baru",
            data: mapDelta(statistics.TimeSeries, d => d.ODP).slice(1)
        }
]