import * as React from "react";
import * as ReactDOM from "react-dom";
import { App } from "./App";
import * as JohnsHopkinsCSSE from "./apis/JohnsHopkinsCSSE";

JohnsHopkinsCSSE.loadStatistics().then(() => {
    ReactDOM.render(
        <App/>,
        document.getElementById("app")
    );
});
