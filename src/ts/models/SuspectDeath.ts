export interface SuspectDeath {
    id: number;
    date: string;
    city: string;
    hospital: string;
    age: number | null;
    status: "Positive" | "Negative" | null;
    officialCause: string | null;
    newsUrl: string;
}