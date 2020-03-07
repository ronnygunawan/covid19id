import * as React from "react";
import { ResponsiveLine, Serie } from "@nivo/line";
import { CartesianMarkerProps } from "@nivo/core"
import { KeyEvent } from "../models/KeyEvent";
import { SuspectDeath } from "../models/SuspectDeath";

interface Props {
    data: Serie[];
    scale: "linear" | "log",
    keyEvents: KeyEvent[] | null;
    suspectDeaths: SuspectDeath[] | null;
}

export const LineChart = ({
    data,
    scale,
    keyEvents,
    suspectDeaths
}: Props) => {
    const markers: CartesianMarkerProps[] = keyEvents !== null && data.length >= 1
        ? keyEvents.map((keyEvent): CartesianMarkerProps => ({
            axis: "x",
            value: data[0].data.find(d => d.x === keyEvent.date) ? keyEvent.date : "TODAY",
            legend: keyEvent.marker,
            textStyle: {
                fontSize: 10,
                fontWeight: "bold",
                letterSpacing: 0.6,
                transform: "rotate(-90deg) translate(-100vh, 0) translate(130px, -10px)"
            }
        }))
        : [];
    if (suspectDeaths !== null && data.length >= 1) {
        const hospitalsByDate = suspectDeaths.reduce<{ [date: string]: string[] }>((agg, cur) => {
            if (cur.status === "Positive") return agg;
            const hospitalAndStatus = cur.status === "Negative"
                ? `${cur.hospital} (Negatif COVID-19)`
                : cur.hospital;
            if (agg[cur.date]) {
                agg[cur.date].push(hospitalAndStatus);
            } else {
                agg[cur.date] = [hospitalAndStatus];
            }
            return agg;
        }, {});
        for (const date in hospitalsByDate) {
            const hospitals = hospitalsByDate[date];
            const legend = hospitals.length === 1
                ? `Kematian suspect di ${hospitals[0]}`
                : hospitals.length === 2
                    ? `Kematian suspect di ${hospitals[0]} dan ${hospitals[1]}`
                    : `Kematian ${hospitals.length} orang suspect`
            markers.push({
                axis: "x",
                value: data[0].data.find(d => d.x === date) ? date : "TODAY",
                legend: legend,
                textStyle: {
                    fontSize: 10,
                    letterSpacing: 0.6,
                    transform: date === "3/6/20"
                        ? "rotate(-90deg) translate(-100vh, 0) translate(300px, -10px)"
                        : "rotate(-90deg) translate(-100vh, 0) translate(130px, -10px)"
                },
                lineStyle: {
                    stroke: "rgba(255, 0, 0, 0.5)"
                }
            });
        }
    }

    if (scale === "log") {
        for (const serie of data) {
            for (let i = 0; i < serie.data.length; i++) {
                if (serie.data[i].y !<= 0) {
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
                max: scale === "log"
                    ? max > 0
                        ? max * 2
                        : 2
                    : "auto",
                stacked: false,
                base: 10
            }}
            axisTop={null}
            axisRight={null}
            axisBottom={{
                orient: "bottom",
                tickSize: 5,
                tickPadding: 5,
                tickRotation: -60,
                legendOffset: 36,
                legendPosition: "middle"
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
            markers={markers} />
    </div>
}