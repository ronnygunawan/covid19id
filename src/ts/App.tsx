import * as React from "react";
import { Serie, Datum } from "@nivo/line";
import { CombinedStatistics } from "./models/CombinedStatistics";
import { KeyEvent } from "./models/KeyEvent";
import { SuspectDeath } from "./models/SuspectDeath";
import * as JohnsHopkinsCSSE from "./apis/JohnsHopkinsCSSE";
import * as Kawalcovid19id from "./apis/Kawalcovid19id";
import * as KeyEventsCSV from "./apis/KeyEventsCSV";
import * as SuspectDeathsCSV from "./apis/SuspectDeathsCSV";
import { LocationSelector } from "./components/LocationSelector";
import { ViewSelector, View } from "./components/ViewSelector";
import { ModeSelector, Mode } from "./components/ModeSelector";
import { ScaleSelector, Scale } from "./components/ScaleSelector";
import { LineChart } from "./components/LineChart";
import { CountdownMudik } from "./components/CountdownMudik";
import { toCumulativeCases, toActiveCases, toNewCases, toCFR1, toCFR2 } from "./transformers";

export const App = () => {
    const [statistics, setStatistics] = React.useState<CombinedStatistics | null>(null);
    const [keyEvents, setKeyEvents] = React.useState<KeyEvent[] | null>(null);
    const [suspectDeaths, setSuspectDeaths] = React.useState<SuspectDeath[] | null>(null);
    const [showViewSelector, setShowViewSelector] = React.useState<boolean>(false);
    const [view, setView] = React.useState<View>("normal");
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
            ? toCumulativeCases(statistics, view)
            : mode === "Belum Sembuh"
                ? toActiveCases(statistics, view)
                : mode === "Kasus Baru"
                    ? toNewCases(statistics, view)
                    : mode === "CFR1"
                        ? toCFR1(statistics, cfrDelay)
                        : mode === "CFR2"
                            ? toCFR2(statistics, cfrDelay)
                            : []
        : [];

    return <>
        <div className="row">
            <div className="col-auto">
                <LocationSelector onChange={(province, country) => {
                    setStatistics(null);
                    setKeyEvents(null);
                    setSuspectDeaths(null);
                    setShowViewSelector(false);
                    setView("normal");
                    JohnsHopkinsCSSE.getStatistics(province, country).then(statistics => {
                        if (country === "Indonesia") {
                            if (statistics !== null && statistics.TimeSeries.length > 0) {
                                Kawalcovid19id.getStatistics().then(kcovidStatistics => {
                                    if (kcovidStatistics !== null && kcovidStatistics.TimeSeries.length === 1) {
                                        const stat = {...statistics};
                                        if (stat.TimeSeries[stat.TimeSeries.length - 1].Date === "TODAY") {
                                            stat.TimeSeries[stat.TimeSeries.length - 1] = kcovidStatistics.TimeSeries[0];
                                        } else {
                                            stat.TimeSeries.push(kcovidStatistics.TimeSeries[0]);
                                        }
                                        setStatistics(stat);
                                    } else {
                                        setStatistics(statistics);
                                    }
                                });
                                return;
                            }
                        }
                        setStatistics(statistics);
                    });
                    KeyEventsCSV.getKeyEvents(country).then(setKeyEvents);
                    if (country === "Indonesia") {
                        SuspectDeathsCSV.getSuspectDeaths().then(setSuspectDeaths);
                        setShowViewSelector(true);
                        setView("mudik");
                    }
                }} setRealtimeStatisticsLoaded={setRealtimeStatisticsLoaded} />
            </div>
            <div className="col-auto">
                {showViewSelector &&
                    <ViewSelector
                        view={view}
                        onChange={setView} />
                }
            </div>
            <div className="col">
                {view === "mudik" &&
                    <CountdownMudik />
                }
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
            view={view}
            data={data}
            scale={scale}
            keyEvents={keyEvents}
            suspectDeaths={suspectDeaths} />
        {realtimeStatisticsLoaded === false &&
            <div className="notice">Data terbaru tidak tersedia. Cobalah pakai incognito.</div>}
    </>;
};