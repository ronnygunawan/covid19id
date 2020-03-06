export interface CombinedStatistics {
    Province_State: string | null;
    Country_Region: string;
    Lat: number;
    Long: number;
    TimeSeries: {
        Date: string;
        Confirmed: number;
        Deaths: number;
        Recovered: number;
    }[];
}