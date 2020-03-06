import * as React from "react";
import { Serie } from "@nivo/line";
import { CombinedStatistics } from "./models/CombinedStatistics";
import * as JohnHopkinsCSSE from "./apis/JohnHopkinsCSSE";
import { LocationSelector } from "./components/LocationSelector";
import { Chart } from "./components/Chart";

export const App = () => {
    const [statistics, setStatistics] = React.useState<CombinedStatistics | null>(null);

    const data: Serie[] = statistics !== null
        ? [
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
        ]
        : [];

    return <>
        <LocationSelector onChange={(province, country) => {
            setStatistics(null);
            JohnHopkinsCSSE.getStatistics(province, country).then(setStatistics);
        }} />
        <Chart data={data} />
    </>;
};