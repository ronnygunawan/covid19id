import * as React from "react";
import { Serie, Datum } from "@nivo/line";
import { CombinedStatistics } from "./models/CombinedStatistics";
import { KeyEvent } from "./models/KeyEvent";
import { SuspectDeath } from "./models/SuspectDeath";
import * as JohnHopkinsCSSE from "./apis/JohnHopkinsCSSE";
import * as KeyEventsCSV from "./apis/KeyEventsCSV";
import * as SuspectDeathsCSV from "./apis/SuspectDeathsCSV";
import { LocationSelector } from "./components/LocationSelector";
import { ModeSelector, Mode } from "./components/ModeSelector";
import { ScaleSelector, Scale } from "./components/ScaleSelector";
import { LineChart } from "./components/LineChart";

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

export const App = () => {
    const [statistics, setStatistics] = React.useState<CombinedStatistics | null>(null);
    const [keyEvents, setKeyEvents] = React.useState<KeyEvent[] | null>(null);
    const [suspectDeaths, setSuspectDeaths] = React.useState<SuspectDeath[] | null>(null);
    const [mode, setMode] = React.useState<Mode>("Kumulatif");
    const [scale, setScale] = React.useState<Scale>("linear");

    const data: Serie[] = statistics !== null
        ? mode === "Kumulatif"
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
            : mode === "Belum Sembuh"
                ? [
                    {
                        id: "Belum Sembuh",
                        data: statistics.TimeSeries.map(t => ({
                            x: t.Date,
                            y: t.Confirmed - t.Deaths - t.Recovered
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
                : mode === "Kasus Baru"
                    ? [
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
                    ]
                    : []
        : [];

    return <>
        <div className="row">
            <div className="col">
                <LocationSelector onChange={(province, country) => {
                    setStatistics(null);
                    setKeyEvents(null);
                    setSuspectDeaths(null);
                    JohnHopkinsCSSE.getStatistics(province, country).then(setStatistics);
                    if (country !== null) {
                        KeyEventsCSV.getKeyEvents(country).then(setKeyEvents);
                    }
                    if (country === "Indonesia") {
                        SuspectDeathsCSV.getSuspectDeaths().then(setSuspectDeaths);
                    }
                }} />
            </div>
            <div className="col-auto">
                <ModeSelector
                    mode={mode}
                    onChange={setMode} />
            </div>
            <div className="col-auto">
                <ScaleSelector
                    scale={scale}
                    onChange={setScale} />
            </div>
        </div>
        <LineChart
            data={data}
            scale={scale}
            keyEvents={keyEvents}
            suspectDeaths={suspectDeaths} />
    </>;
};