import * as React from "react";
import * as JohnHopkinsCSSE from "../apis/JohnHopkinsCSSE";
import { Dropdown, DropdownToggle, DropdownMenu, DropdownItem } from "reactstrap";

interface Props {
    onChange: (province: string | null, country: string) => void;
}

const countryShortlist = ["Indonesia", "Mainland China", "South Korea", "Iran", "Italy", "Japan"];

export const LocationSelector = ({
    onChange
}: Props) => {
    const [country, setCountry] = React.useState<string>("Indonesia");
    const [province, setProvince] = React.useState<string | null>(null);
    const [countries, setCountries] = React.useState<[string, number][] | null>(null);
    const [provinces, setProvinces] = React.useState<[string, number][] | null>(null);
    const [dropdownState, setDropdownState] = React.useState<"country" | "province" | null>(null);

    React.useEffect(() => {
        JohnHopkinsCSSE.getCountries().then(setCountries);
    }, []);

    React.useEffect(() => {
        setProvinces(null);
        JohnHopkinsCSSE.getProvinces(country).then(setProvinces);
    }, [country]);

    React.useEffect(() => {
        onChange(province, country);
    }, [country, province]);

    return <div id="location-selector">
        {countries !== null && <Dropdown
            isOpen={dropdownState === "country"}
            toggle={() => setDropdownState(dropdownState === "country" ? null : "country")}>
            <DropdownToggle caret>
                {country}
            </DropdownToggle>
            <DropdownMenu>
                {countryShortlist.map((country, index) => {
                    const [name, confirmed] = countries.find(([name]) => name === country)!;
                    return <DropdownItem key={index} onClick={() => setCountry(country)}>
                        <div className="row">
                            <div className="col">{name}</div>
                            <div className="col-auto">{confirmed.toLocaleString("id")}</div>
                        </div>
                    </DropdownItem>;
                })}
                <DropdownItem divider />
                {countries.map((country, index) => {
                    const [name, confirmed] = country;
                    return <DropdownItem key={index} onClick={() => setCountry(name)}>
                        <div className="row">
                            <div className="col">{name}</div>
                            <div className="col-auto">{confirmed.toLocaleString("id")}</div>
                        </div>
                    </DropdownItem>;
                })}
            </DropdownMenu>
        </Dropdown>}
        {provinces !== null && provinces.length > 0 && <Dropdown
            isOpen={dropdownState === "province"}
            toggle={() => setDropdownState(dropdownState === "province" ? null : "province")}>
            <DropdownToggle caret>
                {province !== null ? province : "NASIONAL"}
            </DropdownToggle>
            <DropdownMenu>
                <DropdownItem onClick={() => setProvince(null)}>NASIONAL</DropdownItem>
                {provinces.map((province, index) => {
                    const [name, confirmed] = province;
                    return <DropdownItem key={index} onClick={() => setProvince(name)}>
                        <div className="row">
                            <div className="col">{name}</div>
                            <div className="col-auto">{confirmed.toLocaleString("id")}</div>
                        </div>
                    </DropdownItem>;
                })}
            </DropdownMenu>
        </Dropdown>}
    </div>;
}