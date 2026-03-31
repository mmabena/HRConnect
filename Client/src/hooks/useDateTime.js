import  { useState, useEffect } from "react";

const useDateTime = () => {
      const [currentTime, setCurrentTime] = useState("");
  const [currentDate, setCurrentDate] = useState("");

  // -------------------
  // Date & Time
  // -------------------
  useEffect(() => {
    const updateDateTime = () => {
      const now = new Date();
      const month = now.toLocaleDateString("en-ZA", { month: "short" });
      const day = now.toLocaleDateString("en-ZA", { day: "2-digit" });
      const year = now.toLocaleDateString("en-ZA", { year: "numeric" });
      const time = now.toLocaleTimeString("en-ZA", {
        hour: "2-digit",
        minute: "2-digit",
        hour12: false,
      });
      setCurrentDate(`${month}. ${day}, ${year}`);
      setCurrentTime(time);
    };
    updateDateTime();
    const intervalId = setInterval(updateDateTime, 60000);
    return () => clearInterval(intervalId);
  }, []);

  return { currentDate, currentTime };
};

export default useDateTime;