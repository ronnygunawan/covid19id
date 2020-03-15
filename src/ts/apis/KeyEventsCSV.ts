import { KeyEvent } from "../models/KeyEvent";
import * as Papa from "papaparse";

const keyEventsUrl = "https://raw.githubusercontent.com/ronnygunawan/covid19id/master/data/keyevents.csv";

async function _getKeyEvents(): Promise<KeyEvent[]> {
    const response = await fetch(keyEventsUrl);
    const csv = await response.text();
    const csvData: Papa.ParseResult = Papa.parse(csv, {
        header: false
    });
    const [header, ...csvRows]: string[][] = csvData.data;
    const records: KeyEvent[] = [];
    for (const csvRow of csvRows) {
        const [date, country, marker, description, newsUrl] = csvRow;
        const record: KeyEvent = {
            date,
            country: country !== "" ? country : null,
            marker,
            description,
            newsUrl: newsUrl !== "" ? newsUrl : null
        };
        records.push(record);
    }
    return records;
}

var keyEvents: KeyEvent[] | null = null;

export async function loadKeyEvents(): Promise<void> {
    keyEvents = await _getKeyEvents();
}

export async function getKeyEvents(country_region: string | null): Promise<KeyEvent[]> {
    if (keyEvents === null) {
        keyEvents = await _getKeyEvents();
    }
    return keyEvents.filter(k => k.country === null || k.country === country_region);
}