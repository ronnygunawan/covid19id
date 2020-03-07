import * as React from "react";
import { Dropdown, DropdownToggle, DropdownMenu, DropdownItem } from "reactstrap";

export type Mode = "Kumulatif" | "Belum Sembuh" | "Kasus Baru";

interface Props {
    mode: Mode;
    onChange: (mode: Mode) => void;
}

export const ModeSelector = ({
    mode,
    onChange
}: Props) => {
    const [isOpen, setIsOpen] = React.useState<boolean>(false);

    return <div id="mode-selector">
        <Dropdown
            isOpen={isOpen}
            toggle={() => setIsOpen(!isOpen)}>
            <DropdownToggle caret>
                {mode}
            </DropdownToggle>
            <DropdownMenu right>
                {["Kumulatif", "Belum Sembuh", "Kasus Baru"].map((mode: Mode, index) => {
                    return <DropdownItem key={index} onClick={() => onChange(mode)}>
                        {mode}
                    </DropdownItem>
                })}
            </DropdownMenu>
        </Dropdown>
    </div>;
};