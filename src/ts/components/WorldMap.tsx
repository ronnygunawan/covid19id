import * as React from "react";
import { ComposableMap, Graticule, Geographies, Geography } from "react-simple-maps";

export const WorldMap = () => {
    return <ComposableMap
        projectionConfig={{
            rotate: [-10, 0, 0],
            scale: 147
        }}>
        <Geographies geography="world-110m.json">
            {({ geographies }) => {
                return geographies.map(geo => {
                    return <Geography
                        key={geo.rmsKey}
                        geography={geo}
                        fill="#eaeaec"
                        stroke="#d6d6da" />;
                });
            }}
        </Geographies>
    </ComposableMap>;
}