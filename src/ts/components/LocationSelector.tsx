import * as React from "react";
import * as JohnHopkinsCSSE from "../apis/JohnHopkinsCSSE";
import { Dropdown, DropdownToggle, DropdownMenu, DropdownItem } from "reactstrap";

interface Props {
    onChange: (province: string | null, country: string | null) => void;
}

const countryShortlist = ["Indonesia", "Mainland China", "South Korea", "Iran", "Italy", "Japan"];

export const LocationSelector = ({
    onChange
}: Props) => {
    const [country, setCountry] = React.useState<string | null>("Indonesia");
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
        if (country !== null) {
            JohnHopkinsCSSE.getProvinces(country).then(setProvinces);
        }
    }, [country]);

    const worldwideConfirmed = countries !== null
        ? countries.reduce<number>((agg, cur) => {
            const [name, confirmed] = cur;
            return agg + confirmed;
        }, 0)
        : null;

    const outsideChinaConfirmed = countries !== null
        ? countries.filter(([name]) => name !== "Mainland China").reduce<number>((agg, cur) => {
            const [name, confirmed] = cur;
            return agg + confirmed;
        }, 0)
        : null;

    return <div id="location-selector">
        {countries !== null && <Dropdown
            isOpen={dropdownState === "country"}
            toggle={() => setDropdownState(dropdownState === "country" ? null : "country")}>
            <DropdownToggle caret>
                {country || "SELURUH DUNIA"}
            </DropdownToggle>
            <DropdownMenu>
                <DropdownItem onClick={() => {
                    setCountry(null);
                    setProvince(null);
                    onChange(null, null);
                }}>
                    <div className="row">
                        <div className="col">SELURUH DUNIA</div>
                        <div className="col-auto">{worldwideConfirmed !== null ? worldwideConfirmed.toLocaleString("id") : ""}</div>
                    </div>
                </DropdownItem>
                <DropdownItem onClick={() => {
                    setCountry("DI LUAR CHINA");
                    setProvince(null);
                    onChange(null, "DI LUAR CHINA");
                }}>
                    <div className="row">
                        <div className="col">DI LUAR CHINA</div>
                        <div className="col-auto">{outsideChinaConfirmed !== null ? outsideChinaConfirmed.toLocaleString("id") : ""}</div>
                    </div>
                </DropdownItem>
                <DropdownItem divider />
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