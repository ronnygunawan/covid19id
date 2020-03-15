import * as React from "react";
import { ComposableMap, Graticule, Geographies, Geography } from "react-simple-maps";

const geoUrl = "https://raw.githubusercontent.com/zcreativelabs/react-simple-maps/master/topojson-maps/world-110m.json";

export const WorldMap = () => {
    return <ComposableMap
        projection="geoEqualEarth"
        projectionConfig={{
            rotate: [-10, 0, 0],
            scale: 147
        }}>
        <Geographies geography={geoUrl}>
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