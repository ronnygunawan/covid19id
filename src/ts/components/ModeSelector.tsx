import * as React from "react";
import { Dropdown, DropdownToggle, DropdownMenu, DropdownItem } from "reactstrap";

export type Mode = "Kumulatif" | "Belum Sembuh" | "Kasus Baru" | "Observasi" | "Observasi Harian" | "Persentase Observasi Positif" | "Case Fatality Rate (CFR)" | "CFR1" | "CFR2";

interface Props {
    isIndonesia: boolean;
    mode: Mode;
    onChange: (mode: Mode) => void;
}

export const ModeSelector = ({
    isIndonesia,
    mode,
    onChange
}: Props) => {
    const [isOpen, setIsOpen] = React.useState<boolean>(false);

    const options = isIndonesia
        ? ["Kumulatif", "Belum Sembuh", "Kasus Baru", "Observasi", "Observasi Harian", "Persentase Observasi Positif", "Case Fatality Rate (CFR)"]
        : ["Kumulatif", "Belum Sembuh", "Kasus Baru"];

    return <div id="mode-selector">
        <Dropdown
            isOpen={isOpen}
            toggle={() => setIsOpen(!isOpen)}>
            <DropdownToggle caret>
                {mode}
            </DropdownToggle>
            <DropdownMenu right>
                {options.map((mode: Mode, index) => {
                    return <DropdownItem key={index} onClick={() => onChange(mode)}>
                        {mode}
                    </DropdownItem>;
                })}
            </DropdownMenu>
        </Dropdown>
    </div>;
};