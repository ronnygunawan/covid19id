import { SuspectDeath } from "../models/SuspectDeath";

const suspectDeathsUrl = "https://raw.githubusercontent.com/ronnygunawan/covid19id/master/data/suspectdeaths.csv";

var suspectDeaths: SuspectDeath[] | null = null;

export async function getSuspectDeaths(): Promise<SuspectDeath[]> {
    if (suspectDeaths === null) {
        const response = await fetch(suspectDeathsUrl);
        const csv = await response.text();
        const [, ...lines] = csv.split("\n");
        const records: SuspectDeath[] = [];
        for (const line of lines) {
            const [id, date, city, hospital, age, status, officialCause, newsUrl] = line.split(",");
            const record: SuspectDeath = {
                id: parseInt(id),
                date,
                city,
                hospital,
                age: age !== "" ? parseInt(age) : null,
                status: status === "Positive" || status === "Negative" ? status : null,
                officialCause: officialCause !== "" ? officialCause : null,
                newsUrl
            };
            records.push(record);
        }
        suspectDeaths = records;
    }
    return suspectDeaths;
}