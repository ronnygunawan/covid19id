import * as React from "react";
import * as ReactDOM from "react-dom";
import { App } from "./App";
import * as JohnHopkinsCSSE from "./apis/JohnHopkinsCSSE";

JohnHopkinsCSSE.loadStatistics().then(() => {
    ReactDOM.render(
        <App/>,
        document.getElementById("app")
    );
});
