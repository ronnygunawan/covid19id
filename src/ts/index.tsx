import * as React from "react";
import * as ReactDOM from "react-dom";
import { App } from "./App";
import * as JohnsHopkinsCSSE from "./apis/JohnsHopkinsCSSE";
import * as tests from "./tests";

async function testAll(): Promise<void> {
    console.log("Realtime Statistics: " + (await tests.testRealtimeStatistics() || "OK"));
    console.log("Historical Statistics: " + (await tests.testHistoricalStatistics() || "OK"));
    console.log("Combined Statistics: " + (await tests.testCombinedStatistics() || "OK"));
}

(window as any).test = function() {
    testAll();
}

JohnsHopkinsCSSE.loadStatistics().then(() => {
    ReactDOM.render(
        <App/>,
        document.getElementById("app")
    );
});
