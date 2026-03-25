import { useState, useEffect } from "react";

/**
 * Custom React hook that retrieves and formats the current
 * date and time when the component using it mounts.
 *
 * @returns {Object} An object containing:
 * - currentTime: formatted current time (e.g., "3:45 PM")
 * - currentDate: formatted current date (e.g., "Mar 12, 2026")
 */
const useEmpDateTime = () => {
  const [currentTime, setCurrentTime] = useState("");
  const [currentDate, setCurrentDate] = useState("");
/**
   * Runs once when the component mounts.
   * Retrieves the current system date and time
   * and formats them for display.
   */
  useEffect(() => {
    const now = new Date();

    const timeOptions = { hour: "numeric", minute: "2-digit", hour12: true };
    const formattedTime = now.toLocaleTimeString("en-US", timeOptions);

    const dateOptions = { year: "numeric", month: "short", day: "numeric" };
    const formattedDate = now.toLocaleDateString("en-US", dateOptions);

    setCurrentTime(formattedTime);
    setCurrentDate(formattedDate);
  }, []);

  return { currentTime, currentDate };
};

export default useEmpDateTime;