import { Datum } from "@nivo/line";
import { CombinedStatistics } from "../models/CombinedStatistics";

interface DailyStatistics {
    Date: string;
    Confirmed: number;
    Deaths: number;
    Recovered: number;
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

export const toNewCases = (statistics: CombinedStatistics) => [
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
    }
];