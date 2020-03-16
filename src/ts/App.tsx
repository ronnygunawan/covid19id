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
import { toCumulativeCases, toActiveCases, toNewCases, toCFR1, toCFR2, toObservations, toDailyObservations, toConfirmedPercentage, toCaseFatalityRate } from "./transformers";

export const App = () => {
    const [darkMode, setDarkMode] = React.useState<boolean>(true);
    const [statistics, setStatistics] = React.useState<CombinedStatistics | null>(null);
    const [keyEvents, setKeyEvents] = React.useState<KeyEvent[] | null>(null);
    const [suspectDeaths, setSuspectDeaths] = React.useState<SuspectDeath[] | null>(null);
    const [isIndonesia, setIsIndonesia] = React.useState<boolean>(false);
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
                    : mode === "Observasi"
                        ? toObservations(statistics, view)
                        : mode === "Observasi Harian"
                            ? toDailyObservations(statistics, view)
                            : mode === "Persentase Observasi Positif"
                                ? toConfirmedPercentage(statistics, view)
                                : mode === "Case Fatality Rate (CFR)"
                                    ? toCaseFatalityRate(statistics, view)
                                    : mode === "CFR1"
                                        ? toCFR1(statistics, cfrDelay)
                                        : mode === "CFR2"
                                            ? toCFR2(statistics, cfrDelay)
                                            : []
        : [];

    React.useEffect(() => {
        document.body.className = darkMode ? "theme-dark" : "theme-light";
    }, [darkMode]);

    return <>
        <div className="row">
            <div className="col-auto">
                <LocationSelector onChange={(province, country) => {
                    setStatistics(null);
                    setKeyEvents(null);
                    setSuspectDeaths(null);
                    setIsIndonesia(false);
                    setView("normal");
                    JohnsHopkinsCSSE.getStatistics(province, country).then(statistics => {
                        if (country === "Indonesia") {
                            if (statistics !== null && statistics.TimeSeries.length > 0) {
                                Kawalcovid19id.getStatistics().then(kcovidStatistics => {
                                    const stat = { ...statistics };
                                    if (kcovidStatistics !== null && kcovidStatistics.TimeSeries.length === 1) {
                                        if (stat.TimeSeries[stat.TimeSeries.length - 1].Date === "TODAY") {
                                            stat.TimeSeries[stat.TimeSeries.length - 1] = kcovidStatistics.TimeSeries[0];
                                        } else {
                                            stat.TimeSeries.push(kcovidStatistics.TimeSeries[0]);
                                        }
                                    }
                                    Kawalcovid19id.getDailyStatistics().then(kcovidDailyStatistics => {
                                        if (kcovidDailyStatistics !== null) {
                                            for (const kcovidStat of kcovidDailyStatistics) {
                                                const ts = stat.TimeSeries.find(ts => ts.Date === kcovidStat.date);
                                                if (ts) {
                                                    ts.Observed = kcovidStat.observed;
                                                    ts.Negatives = kcovidStat.negatives;
                                                } else {
                                                    const todayTs = stat.TimeSeries.find(ts => ts.Date === "TODAY");
                                                    if (todayTs) {
                                                        todayTs.Observed = kcovidStat.observed;
                                                        todayTs.Negatives = kcovidStat.negatives;
                                                    }
                                                }
                                            }
                                        }
                                        setStatistics(stat);
                                    }).catch(() => setStatistics(stat));
                                });
                                return;
                            }
                        } else {
                            if (mode !== "Kumulatif"
                                && mode !== "Belum Sembuh"
                                && mode !== "Kasus Baru") {
                                setMode("Kumulatif");
                            }
                        }
                        setStatistics(statistics);
                    });
                    KeyEventsCSV.getKeyEvents(country).then(setKeyEvents);
                    if (country === "Indonesia") {
                        SuspectDeathsCSV.getSuspectDeaths().then(setSuspectDeaths);
                        setIsIndonesia(true);
                        setView("mudik");
                    }
                }} setRealtimeStatisticsLoaded={setRealtimeStatisticsLoaded} />
            </div>
            <div className="col-auto">
                {isIndonesia &&
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
                    isIndonesia={isIndonesia}
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
            darkMode={darkMode}
            country={statistics?.Country_Region || null}
            view={view}
            mode={mode}
            data={data}
            scale={scale}
            keyEvents={keyEvents}
            suspectDeaths={suspectDeaths} />
        {realtimeStatisticsLoaded === false &&
            <div className="notice">Data terbaru tidak tersedia. Cobalah pakai incognito.</div>}
    </>;
};