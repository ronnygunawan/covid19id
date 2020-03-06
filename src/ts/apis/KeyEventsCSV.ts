import { KeyEvent } from "../models/KeyEvent";

const keyEventsUrl = "https://github.com/ronnygunawan/covid19id/raw/master/data/keyevents.csv";

async function _getKeyEvents(): Promise<KeyEvent[]> {
    const response = await fetch(keyEventsUrl);
    const csv = await response.text();
    const [, ...lines] = csv.split("\n");
    const records: KeyEvent[] = [];
    for (const line of lines) {
        const [date, country, marker, description, newsUrl] = line.split(",");
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

export async function getKeyEvents(country_region: string): Promise<KeyEvent[]> {
    if (keyEvents === null) {
        keyEvents = await _getKeyEvents();
    }
    return keyEvents.filter(k => k.country === null || k.country === country_region);
}