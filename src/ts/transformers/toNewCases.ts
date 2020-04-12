import { Datum } from "@nivo/line";
import { CombinedStatistics } from "../models/CombinedStatistics";
import { View } from "../components/ViewSelector";

interface DailyStatistics {
    Date: string;
    Confirmed: number;
    Deaths: number;
    Recovered: number | null;
}

const mapDelta = (timeSeries: DailyStatistics[], selector: (x: DailyStatistics) => number): Datum[] => timeSeries.reduce<[Datum[], DailyStatistics | null]>((agg, cur) => {
    const [arr, prev] = agg;
    if (prev === null) {
        arr.push({
            x: cur.Date,
            y: selector(cur)
        });
    } else {
        arr.push({
            x: cur.Date,
            y: selector(cur) - selector(prev)
        });
    }
    return [arr, cur];
}, [[], null])[0];

const toIndonesiaNewCases = (statistics: CombinedStatistics) => {
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
                data: mapDelta(statistics.TimeSeries.filter(d => d.Recovered !== null), d => d.Recovered!)
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

export const toNewCases = (statistics: CombinedStatistics, view: View) => statistics.Country_Region === "Indonesia" && view === "mudik"
    ? toIndonesiaNewCases(statistics)
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
            data: mapDelta(statistics.TimeSeries.filter(d => d.Recovered !== null), d => d.Recovered!)
        }
    ];