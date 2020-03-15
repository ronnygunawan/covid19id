import * as React from "react";

const firstHoliday = new Date(2020, 4, 20, 16, 0, 0, 0);

export const CountdownMudik = () => {
    const divRef = React.useRef<HTMLDivElement | null>(null);

    React.useEffect(() => {
        const interval = window.setInterval(() => {
            const now = new Date();
            if (divRef.current !== null && now.getTime() < firstHoliday.getTime()) {
                const totalSeconds = (firstHoliday.getTime() - now.getTime()) / 1000;
                const seconds = Math.floor(totalSeconds % 60);
                const minutes = Math.floor(totalSeconds / 60) % 60;
                const hours = Math.floor(totalSeconds / 3600) % 24;
                const days = Math.floor(totalSeconds / 86400);
                const texts: string[] = [];
                if (days > 0) {
                    texts.push(`${days} hari`);
                }
                if (hours > 0) {
                    texts.push(`${hours} jam`);
                }
                if (minutes > 0) {
                    texts.push(`${minutes} menit`);
                }
                if (seconds > 0) {
                    texts.push(`${seconds} detik`);
                }
                divRef.current.innerText = texts.join(" ");
            }
        }, 200);
        return () => {
            clearInterval(interval);
        };
    }, []);

    return <div id="countdown-mudik" ref={el => divRef.current = el}></div>
}