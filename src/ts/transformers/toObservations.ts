import { CombinedStatistics } from "../models/CombinedStatistics";
import { View } from "../components/ViewSelector";

const toIndonesiaObservations = (statistics: CombinedStatistics) => {
    const result: {
        id: string,
        data: {
            x: string,
            y: number | null
        }[]
    }[] = [
            {
                id: "Positif",
                data: []
            },
            {
                id: "Meninggal",
                data: []
            },
            {
                id: "Sembuh",
                data: []
            },
            {
                id: "Negatif",
                data: []
            },
            {
                id: "Diperiksa",
                data: []
            },
            {
                id: "PDP",
                data: []
            },
            {
                id: "ODP",
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
                y: t.Confirmed
            });
            result[1].data.push({
                x: t.Date,
                y: t.Deaths
            });
            result[2].data.push({
                x: t.Date,
                y: t.Recovered
            });
            if (t.Negatives !== null) {
                result[3].data.push({
                    x: t.Date,
                    y: t.Negatives
                });
            }
            if (t.Observed !== null) {
                result[4].data.push({
                    x: t.Date,
                    y: t.Observed
                });
            }
            if (t.PDP !== null) {
                result[5].data.push({
                   x: t.Date,
                   y: t.PDP 
                });
            }
            if (t.ODP !== null) {
                result[6].data.push({
                    x: t.Date,
                    y: t.ODP
                });
            }
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

export const toObservations = (statistics: CombinedStatistics, view: View) => statistics.Country_Region === "Indonesia" && view === "mudik"
    ? toIndonesiaObservations(statistics)
    : [
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
            id: "Negatif",
            data: statistics.TimeSeries.filter(t => t.Negatives !== null).map(t => ({
                x: t.Date,
                y: t.Negatives
            }))
        },
        {
            id: "Diperiksa",
            data: statistics.TimeSeries.filter(t => t.Observed !== null).map(t => ({
                x: t.Date,
                y: t.Observed
            }))
        },
        {
            id: "PDP",
            data: statistics.TimeSeries.filter(t => t.PDP !== null).map(t => ({
                x: t.Date,
                y: t.PDP
            }))
        },
        {
            id: "ODP",
            data: statistics.TimeSeries.filter(t => t.ODP !== null).map(t => ({
                x: t.Date,
                y: t.ODP
            }))
        },
    ];