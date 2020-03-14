import * as React from "react";
import { Serie, Datum } from "@nivo/line";
import { CombinedStatistics } from "./models/CombinedStatistics";
import { KeyEvent } from "./models/KeyEvent";
import { SuspectDeath } from "./models/SuspectDeath";
import * as JohnsHopkinsCSSE from "./apis/JohnsHopkinsCSSE";
import * as KeyEventsCSV from "./apis/KeyEventsCSV";
import * as SuspectDeathsCSV from "./apis/SuspectDeathsCSV";
import { LocationSelector } from "./components/LocationSelector";
import { ModeSelector, Mode } from "./components/ModeSelector";
import { ScaleSelector, Scale } from "./components/ScaleSelector";
import { LineChart } from "./components/LineChart";
import { toCumulativeCases, toActiveCases, toNewCases, toCFR1, toCFR2 } from "./transformers";

export const App = () => {
    const [statistics, setStatistics] = React.useState<CombinedStatistics | null>(null);
    const [keyEvents, setKeyEvents] = React.useState<KeyEvent[] | null>(null);
    const [suspectDeaths, setSuspectDeaths] = React.useState<SuspectDeath[] | null>(null);
    const [mode, setMode] = React.useState<Mode>("Kumulatif");
    const [scale, setScale] = React.useState<Scale>("linear");
    const [cfrDelay, setCfrDelay] = React.useState<number>(0);
    const [realtimeStatisticsLoaded, setRealtimeStatisticsLoaded] = React.useState<boolean | null>(null);

    (window as any).cfr1 = function (delay: number) {
        setMode("CFR1");
        setCfrDelay(delay || 0);
    };

    (window as any).cfr2 = function (delay: number) {
        setMode("CFR2");
        setCfrDelay(delay || 0);
    };

    const data: Serie[] = statistics !== null
        ? mode === "Kumulatif"
            ? toCumulativeCases(statistics)
            : mode === "Belum Sembuh"
                ? toActiveCases(statistics)
                : mode === "Kasus Baru"
                    ? toNewCases(statistics)
                    : mode === "CFR1"
                        ? toCFR1(statistics, cfrDelay)
                        : mode === "CFR2"
                            ? toCFR2(statistics, cfrDelay)
                            : []
        : [];

    return <>
        <div className="row">
            <div className="col">
                <LocationSelector onChange={(province, country) => {
                    setStatistics(null);
                    setKeyEvents(null);
                    setSuspectDeaths(null);
                    JohnsHopkinsCSSE.getStatistics(province, country).then(setStatistics);
                    KeyEventsCSV.getKeyEvents(country).then(setKeyEvents);
                    if (country === "Indonesia") {
                        SuspectDeathsCSV.getSuspectDeaths().then(setSuspectDeaths);
                    }
                }} setRealtimeStatisticsLoaded={setRealtimeStatisticsLoaded} />
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
            country={statistics?.Country_Region || null}
            data={data}
            scale={scale}
            keyEvents={keyEvents}
            suspectDeaths={suspectDeaths} />
        {realtimeStatisticsLoaded === false &&
            <div className="notice">Data terbaru tidak tersedia. Cobalah pakai incognito.</div>}
    </>;
};