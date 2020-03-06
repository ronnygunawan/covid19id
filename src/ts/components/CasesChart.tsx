import * as React from "react";
import { ResponsiveLine, Serie } from "@nivo/line";
import { CartesianMarkerProps } from "@nivo/core"
import { KeyEvent } from "../models/KeyEvent";
import { SuspectDeath } from "../models/SuspectDeath";

interface Props {
    data: Serie[];
    keyEvents: KeyEvent[] | null;
    suspectDeaths: SuspectDeath[] | null;
}

export const CasesChart = ({
    data,
    keyEvents,
    suspectDeaths
}: Props) => {
    const markers: CartesianMarkerProps[] = keyEvents !== null && data.length >= 1
        ? keyEvents.map((keyEvent): CartesianMarkerProps => ({
            axis: "x",
            value: data[0].data.find(d => d.x === keyEvent.date) ? keyEvent.date : "TODAY",
            legend: keyEvent.marker,
            textStyle: {
                fontSize: 12
            }
        }))
        : [];
    if (suspectDeaths !== null && data.length >= 1) {
        for (const suspectDeath of suspectDeaths) {
            markers.push({
                axis: "x",
                value: data[0].data.find(d => d.x === suspectDeath.date) ? suspectDeath.date : "TODAY",
                lineStyle: {
                    stroke: "rgba(255, 0, 0, 0.5)"
                }
            })
        }
    }

    return <div id="chart">
        <ResponsiveLine
            data={data}
            margin={{ top: 70, right: 110, bottom: 50, left: 60 }}
            xScale={{ type: "point" }}
            yScale={{ type: "linear", min: 0, max: "auto", stacked: false }}
            axisTop={null}
            axisRight={null}
            axisBottom={{
                orient: "bottom",
                tickSize: 5,
                tickPadding: 5,
                tickRotation: 0,
                legend: "Tanggal (m/d/yy)",
                legendOffset: 36,
                legendPosition: "middle"
            }}
            axisLeft={{
                orient: "left",
                tickSize: 5,
                tickPadding: 5,
                tickRotation: 0,
                legend: "Jumlah Penderita",
                legendOffset: -40,
                legendPosition: "middle"
            }}
            colors={{ scheme: "nivo" }}
            pointSize={10}
            pointColor={{ theme: "background" }}
            pointBorderWidth={2}
            pointBorderColor={{ from: "serieColor" }}
            pointLabel="y"
            pointLabelYOffset={-12}
            enableArea={true}
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