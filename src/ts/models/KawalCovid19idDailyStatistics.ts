export interface KawalCovid19idDailyStatistics {
    date: string;
    newCases: number;
    cases: number;
    activeCases: number;
    newRecoveries: number;
    recovered: number;
    newDeaths: number;
    deceased: number;
    pdp: number | null;
    odp: number | null;
    observed: number;
    confirmed: number;
    negatives: number;
    observing: number;
}