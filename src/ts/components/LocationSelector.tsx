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
        onChange(province, country);
    }, []);

    React.useEffect(() => {
        setProvinces(null);
        JohnHopkinsCSSE.getProvinces(country).then(setProvinces);
    }, [country]);

    return <div id="location-selector">
        {countries !== null && <Dropdown
            isOpen={dropdownState === "country"}
            toggle={() => setDropdownState(dropdownState === "country" ? null : "country")}>
            <DropdownToggle caret>
                {country}
            </DropdownToggle>
            <DropdownMenu>
                {countryShortlist.filter(name => countries.find(([n]) => n === name)).map((c, index) => {
                    const [name, confirmed] = countries.find(([n]) => n === c)!;
                    return <DropdownItem key={index} onClick={() => {
                        if (country !== c) {
                            setCountry(c);
                            setProvince(null);
                            onChange(null, c);
                        }
                    }}>
                        <div className="row">
                            <div className="col">{name}</div>
                            <div className="col-auto">{confirmed.toLocaleString("id")}</div>
                        </div>
                    </DropdownItem>;
                })}
                <DropdownItem divider />
                {countries.map((c, index) => {
                    const [name, confirmed] = c;
                    return <DropdownItem key={index} onClick={() => {
                        if (country !== name) {
                            setCountry(name);
                            setProvince(null);
                            onChange(null, name);
                        }
                    }}>
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
                <DropdownItem onClick={() => {
                    setProvince(null);
                    onChange(null, country);
                }}>NASIONAL</DropdownItem>
                {provinces.map((province, index) => {
                    const [name, confirmed] = province;
                    return <DropdownItem key={index} onClick={() => {
                        setProvince(name);
                        onChange(name, country);
                    }}>
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