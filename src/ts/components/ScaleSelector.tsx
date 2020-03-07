import * as React from "react";
import { Dropdown, DropdownToggle, DropdownMenu, DropdownItem } from "reactstrap";

export type Scale = "linear" | "log";

interface Props {
    scale: Scale;
    onChange: (scale: Scale) => void;
}

const getScaleName = (scale: Scale) => {
    switch (scale) {
        case "linear": return "Linear";
        case "log": return "Logaritmik";
    }
}

export const ScaleSelector = ({
    scale,
    onChange
}: Props) => {
    const [isOpen, setIsOpen] = React.useState<boolean>(false);

    return <div id="scale-selector">
        <Dropdown
            isOpen={isOpen}
            toggle={() => setIsOpen(!isOpen)}>
            <DropdownToggle caret>
                {getScaleName(scale)}
            </DropdownToggle>
            <DropdownMenu right>
                {["linear", "log"].map((scale: Scale, index) => {
                    return <DropdownItem key={index} onClick={() => onChange(scale)}>
                        {getScaleName(scale)}
                    </DropdownItem>
                })}
            </DropdownMenu>
        </Dropdown>
    </div>;
};