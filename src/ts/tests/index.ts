import * as JohnsHopkinsCSSE from "../apis/JohnsHopkinsCSSE";
import { countryShortlist } from "../components/LocationSelector";

export async function testRealtimeStatistics(): Promise<string | null> {
    try {
        const realtimeStatistics = await JohnsHopkinsCSSE.getRealtimeStatistics();
        for (const country of countryShortlist) {
            if (!realtimeStatistics.find(rts => rts.Country_Region === country)) return `${country} is missing.`;
        }
        return null;
    } catch (err) {
        return err;
    }
}

export async function testHistoricalStatistics(): Promise<string | null> {
    try {
        const historicalStatistics = await JohnsHopkinsCSSE.getHistoricalStatistics();
        for (const country of countryShortlist) {
            const hs = historicalStatistics.find(hs => hs.Country_Region === country);
            if (!hs) return `${country} is missing.`;
            if (hs.TimeSeries.length < 40) return `${country} has fewer than 40 days of data.`;
        }
        return null;
    } catch (err) {
        return err;
    }
}

export async function testCombinedStatistics(): Promise<string | null> {
    const realtimeStatistics = await JohnsHopkinsCSSE.getRealtimeStatistics();
    const historicalStatistics = await JohnsHopkinsCSSE.getHistoricalStatistics();
    if (realtimeStatistics.length < historicalStatistics.length) return `Realtime statistics (${realtimeStatistics.length} rows) has fewer rows than historical statistics (${historicalStatistics.length} rows).`;
    return null;
}