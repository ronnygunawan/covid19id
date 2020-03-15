import * as React from "react";
import { Dropdown, DropdownToggle, DropdownMenu, DropdownItem } from "reactstrap";

export type View = "normal" | "mudik";

interface Props {
    view: View;
    onChange: (view: View) => void;
}

const getViewName = (view: View) => {
    switch (view) {
        case "normal": return "Normal";
        case "mudik": return "Countdown Mudik";
    }
}

export const ViewSelector = ({
    view,
    onChange
}: Props) => {
    const [isOpen, setIsOpen] = React.useState<boolean>(false);

    return <div id="view-selector">
        <Dropdown
            isOpen={isOpen}
            toggle={() => setIsOpen(!isOpen)}>
            <DropdownToggle caret>
                {getViewName(view)}
            </DropdownToggle>
            <DropdownMenu>
                {["normal", "mudik"].map((view: View, index) => {
                    return <DropdownItem key={index} onClick={() => onChange(view)}>
                        {getViewName(view)}
                    </DropdownItem>
                })}
            </DropdownMenu>
        </Dropdown>
    </div>;
};