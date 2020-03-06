import * as React from "react";
import { Serie } from "@nivo/line";
import { CombinedStatistics } from "./models/CombinedStatistics";
import { KeyEvent } from "./models/KeyEvent";
import { SuspectDeath } from "./models/SuspectDeath";
import * as JohnHopkinsCSSE from "./apis/JohnHopkinsCSSE";
import * as KeyEventsCSV from "./apis/KeyEventsCSV";
import * as SuspectDeathsCSV from "./apis/SuspectDeathsCSV";
import { LocationSelector } from "./components/LocationSelector";
import { Chart } from "./components/Chart";

export const App = () => {
    const [statistics, setStatistics] = React.useState<CombinedStatistics | null>(null);
    const [keyEvents, setKeyEvents] = React.useState<KeyEvent[] | null>(null);
    const [suspectDeaths, setSuspectDeaths] = React.useState<SuspectDeath[] | null>(null);

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
            setKeyEvents(null);
            setSuspectDeaths(null);
            JohnHopkinsCSSE.getStatistics(province, country).then(setStatistics);
            KeyEventsCSV.getKeyEvents(country).then(setKeyEvents);
            if (country === "Indonesia") {
                SuspectDeathsCSV.getSuspectDeaths().then(setSuspectDeaths);
            }
        }} />
        <Chart
            data={data}
            keyEvents={keyEvents}
            suspectDeaths={suspectDeaths} />
    </>;
};