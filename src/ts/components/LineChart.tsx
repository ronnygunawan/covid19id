import * as React from "react";
import { ResponsiveLine, Serie } from "@nivo/line";
import { CartesianMarkerProps } from "@nivo/core"
import { KeyEvent } from "../models/KeyEvent";
import { SuspectDeath } from "../models/SuspectDeath";
import { View } from "./ViewSelector";
import { Mode } from "./ModeSelector";

interface Props {
    darkMode: boolean;
    country: string | null;
    view: View;
    mode: Mode;
    data: Serie[];
    scale: "linear" | "log",
    keyEvents: KeyEvent[] | null;
    suspectDeaths: SuspectDeath[] | null;
}

const daysOfWeek = ["Minggu", "Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu"];

export const LineChart = ({
    darkMode,
    country,
    view,
    mode,
    data,
    scale,
    keyEvents,
    suspectDeaths
}: Props) => {
    const markers: CartesianMarkerProps[] = keyEvents !== null && data.length >= 1
        ? keyEvents.filter(keyEvent => {
            const [m, d, y] = keyEvent.date.split("/");
            const date = new Date(parseInt(y) + 2000, parseInt(m) - 1, parseInt(d));
            if (country !== "Indonesia" || view !== "mudik") {
                return date.getTime() < Date.now();
            } else {
                return date.getTime() >= new Date(2020, 2, 1).getTime();
            }
        }).map((keyEvent): CartesianMarkerProps => ({
            axis: "x",
            value: data[0].data.find(d => d.x === keyEvent.date) ? keyEvent.date : "TODAY",
            legend: keyEvent.marker,
            textStyle: {
                fontSize: 10,
                fontWeight: "bold",
                letterSpacing: 0.4,
                transform: "rotate(-90deg) translate(-100vh, 0) translate(200px, -7px)"
            },
            lineStyle: {
                stroke: "rgba(0, 0, 0, 0.4)"
            }
        }))
        : [];

    if (scale === "log") {
        for (const serie of data) {
            for (let i = 0; i < serie.data.length; i++) {
                if (serie.data[i].y! <= 0) {
                    serie.data[i] = {
                        x: serie.data[i].x,
                        y: 0.00000000000000000000000000000000000000000001
                    };
                }
            }
        }
    }

    const max = data.reduce<number>((prev, cur) => {
        const max = cur.data.reduce<number>((prev, cur) => {
            const y = cur.y as number;
            if (y > prev) {
                return y;
            }
            return prev;
        }, 0);
        if (max > prev) {
            return max;
        }
        return prev;
    }, 0);

    const yScaleMax: number | "auto" = scale === "log"
        ? max > 0
            ? max * 2
            : 2
        : country === "Indonesia"
            ? mode === "Persentase Observasi Positif"
                ? Math.max(max, 20)
                : mode === "Case Fatality Rate (CFR)"
                    ? Math.max(max, 10)
                    : view === "mudik"
                        ? Math.max(max, 250)
                        : "auto"
            : "auto";

    return <div id="chart">
        <ResponsiveLine
            data={data}
            margin={{ top: 70, right: 120, bottom: 50, left: 80 }}
            xScale={{ type: "point" }}
            yScale={{
                type: scale,
                min: scale === "linear"
                    ? 0
                    : scale === "log"
                        ? 1
                        : "auto",
                max: yScaleMax,
                stacked: false,
                base: 10
            }}
            gridXValues={data[0]?.data.map(d => d.x as string).filter(x => {
                if (x === "TODAY") return true;
                const [m, d, y] = x.split("/");
                const date = new Date(parseInt(y) + 2000, parseInt(m) - 1, parseInt(d));
                return date.getDay() === 0;
            }) || []}
            axisTop={null/*{
                orient: "top",
                tickSize: 5,
                tickPadding: 5,
                tickRotation: -60,
                legendOffset: -36,
                legendPosition: "middle",
                format: (value: string): string => {
                    if (value === "TODAY") return "TODAY";
                    const date = new Date(value);
                    return daysOfWeek[date.getDay()];
                }
            }*/}
            axisRight={null}
            axisBottom={{
                orient: "bottom",
                tickSize: 5,
                tickPadding: 5,
                tickRotation: -60,
                legendOffset: 36,
                legendPosition: "middle",
                format: (x: string): string => {
                    if (x === "TODAY") return x;
                    const [m, d, y] = x.split("/");
                    const date = new Date(parseInt(y) + 2000, parseInt(m) - 1, parseInt(d));
                    return date.getDay() === 0 ? x : "";
                }
            }}
            axisLeft={{
                orient: "left",
                tickSize: 5,
                tickPadding: 5,
                tickRotation: 0,
                legend: "Jumlah Penderita",
                legendOffset: -60,
                legendPosition: "middle"
            }}
            colors={{ scheme: "category10" }}
            pointSize={10}
            pointColor={{ theme: "background" }}
            pointBorderWidth={2}
            pointBorderColor={{ from: "serieColor" }}
            pointLabel="y"
            pointLabelYOffset={-12}
            yFormat={(value: number) => mode === "Persentase Observasi Positif" || mode === "Case Fatality Rate (CFR)"
                ? `${value.toFixed(1)}%`
                : value}
            enableArea={scale === "linear"}
            useMesh={true}
            legends={[
                {
                    anchor: 'bottom-right',
                    direction: 'column',
                    justify: false,
                    translateX: 100,
                    translateY: 0,
                    itemsSpacing: 0,
                    itemDirection: 'left-to-right',
                    itemWidth: 80,
                    itemHeight: 20,
                    itemOpacity: 0.75,
                    symbolSize: 12,
                    symbolShape: 'circle',
                    symbolBorderColor: 'rgba(0, 0, 0, .5)',
                    effects: [
                        {
                            on: 'hover',
                            style: {
                                itemBackground: 'rgba(0, 0, 0, .03)',
                                itemOpacity: 1
                            }
                        }
                    ]
                }
            ]}
            markers={markers}
            animate={false}
            layers={country === "Indonesia" && view === "mudik"
                ? [
                    "grid",
                    "axes",
                    ({ xScale, yScale }) => {
                        const left = xScale("5/21/20");
                        const right = xScale("5/27/20");
                        const top = yScaleMax === "auto"
                            ? yScale(max)
                            : yScale(Math.max(max, yScaleMax));
                        const bottom = yScale(0);
                        return <>
                            <path d={`M${left},${bottom} ${left},${top} ${right},${top} ${right},${bottom}Z`} fill="rgba(255, 0, 0, 0.2)" />
                            <text x={left} y={top - 4} fill="rgba(128, 0, 0, 1)" fontSize="13">ARUS MUDIK</text>
                        </>;
                    },
                    "areas",
                    "lines",
                    "points",
                    "markers",
                    "mesh",
                    "legends"
                ]
                : undefined} />
    </div>
}