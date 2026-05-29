import { useState, useEffect } from "react";
import React from "react";
import api from "../../api/api";
import "./AddBenchmark.css";

const LOCATIONS = ["Johannesburg", "Cape Town"];

function emptyCard() {
  return {
    salary25th: "",
    salary50th: "",
    salary75th: "",
    source: "",
    year: "",
  };
}

function AddBenchmark({ onClose, onAddSuccess }) {
  const [positions, setPositions] = useState([]);
  const [loadingPositions, setLoadingPositions] = useState(true);
  const [positionId, setPositionId] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [conflicts, setConflicts] = useState([]);
  const [currentLocation, setCurrentLocation] = useState(0);
  const currentLoc = LOCATIONS[currentLocation];

  //one benchmark per location
  const [cards, setCards] = useState({
    Johannesburg: emptyCard(),
    "Cape Town": emptyCard(),
  });

  const [errors, setErrors] = useState({
    Johannesburg: {},
    "Cape Town": {},
  });

  useEffect(() => {
    const fetchPositions = async () => {
      try {
        const res = await api.get("/positions");
        setPositions(res.data);
      } catch (err) {
        console.error("Failed to load positions", err);
        setError("Failed to load positions");
      } finally {
        setLoadingPositions(false);
      }
    };

    fetchPositions();
  }, []);

  //checking for exisiting benchmarks when position is selected
  useEffect(() => {
    if (!positionId) {
      setConflicts([]);
      return;
    }

    async function checkConflicts() {
      try {
        // fetch all benchmarks and filter client-side
        // avoids needing a new endpoint
        const res = await api.get("/salary-benchmarks");
        const existing = res.data.filter(
          (b) => b.positionId === Number(positionId),
        );
        setConflicts(existing.map((b) => b.location));
      } catch {
        // non-critical — just skip conflict check
        setConflicts([]);
      }
    }

    checkConflicts();
  }, [positionId]);

  function handleCardChange(location, field, value) {
    setCards((prev) => ({
      ...prev,
      [location]: { ...prev[location], [field]: value },
    }));
    setErrors((prev) => ({
      ...prev,
      [location]: { ...prev[location], [field]: "" },
    }));
  }

  function validate() {
    if (!positionId) {
      setError("Please select a position.");
      return false;
    }

    let valid = true;
    const newErrors = { Johannesburg: {}, "Cape Town": {} };

    LOCATIONS.forEach((loc) => {
      const c = cards[loc];

      if (!c.year.trim()) newErrors[loc].year = "Select Date";
      if (!c.source.trim()) newErrors[loc].source = "Required";

      if (!c.salary25th) newErrors[loc].salary25th = "Required";
      else if (Number(c.salary25th) <= 0)
        newErrors[loc].salary25th = "Must be greater than 0";

      if (!c.salary50th) newErrors[loc].salary50th = "Required";
      else if (Number(c.salary50th) <= 0)
        newErrors[loc].salary50th = "Must be greater than 0";

      if (!c.salary75th) newErrors[loc].salary75th = "Required";
      else if (Number(c.salary75th) <= 0)
        newErrors[loc].salary75th = "Must be greater than 0";

      // percentile order
      if (Number(c.salary25th) >= Number(c.salary50th))
        newErrors[loc].salary50th = "P50 must be greater than P25";
      if (Number(c.salary50th) >= Number(c.salary75th))
        newErrors[loc].salary75th = "P75 must be greater than P50";

      if (Object.keys(newErrors[loc]).length > 0) valid = false;
    });

    setErrors(newErrors);
    return valid;
  }

  function handleReset() {
    setPositionId("");
    setCurrentLocation(0);
    setCards({ Johannesburg: emptyCard(), "Cape Town": emptyCard() });
    setErrors({ Johannesburg: {}, "Cape Town": {} });
    setError(null);
    setConflicts([]);
  }

  // auto close after success
  useEffect(() => {
    if (success) {
      const t = setTimeout(() => {
        setSuccess(null);
        onClose();
      }, 4000);
      return () => clearTimeout(t);
    }
  }, [success]);

  async function handleSave() {
    if (saving) return;
    setError(null);

    if (!validate()) return;

    const blocked = LOCATIONS.filter((loc) => conflicts.includes(loc));
    if (blocked.length > 0) {
      setError(
        `A benchmark already exists for ${blocked.join(" and ")} for this position. Please edit the existing one instead.`,
      );
      return;
    }

    setSaving(true);

    const failed = [];
    const succeeded = [];

    for (const loc of LOCATIONS) {
      try {
        const payload = {
          PositionId: Number(positionId),
          Location: loc,
          Salary25th: Number(cards[loc].salary25th),
          Salary50th: Number(cards[loc].salary50th),
          Salary75th: Number(cards[loc].salary75th),
          Source: cards[loc].source.trim(),
          Year: Number(cards[loc].year),
        };

        console.log(`Sending ${loc}:`, payload);

        await api.post("/salary-benchmarks", payload);
        succeeded.push(loc);
      } catch (err) {
        console.log(`Failed ${loc}:`, err.response?.status, err.response?.data);
        failed.push(loc);
      }
    }

    setSaving(false);

    if (failed.length === 0) {
      setSuccess("Both benchmarks saved successfully!");
      onAddSuccess?.();
    } else if (succeeded.length > 0) {
      setError(
        `${succeeded.join(" and ")} saved but ${failed.join(" and ")} failed. Please try again.`,
      );
      onAddSuccess?.();
    } else {
      setError("Failed to save benchmarks. Please try again.");
    }
  }

  const currentYear = new Date().getFullYear();

  const YEARS = Array.from(
    { length: currentYear - 2019 + 1,},
    (_, i) => currentYear - i
  );
  
 const loc = currentLoc;
          const card = cards[loc];
          const cardErrors = errors[loc];
          const hasConflict = conflicts.includes(loc);
  return (
    <div className="b-container">
      <div className="b-head-container">
        <button className="b-x-btn" onClick={onClose}>
          ✕
        </button>
        <div className="b-title-header">
          <span className="b-title">
            <svg
              width="34"
              height="34"
              viewBox="0 0 34 34"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <rect
                width="34"
                height="34"
                rx="10"
                fill="white"
                fill-opacity="0.15"
              />
              <path
                d="M17 21.5V26.5M21 19.5V26.5M25 15.5V26.5M27 8.5L18.354 17.146C18.3076 17.1926 18.2524 17.2295 18.1916 17.2547C18.1309 17.2799 18.0658 17.2929 18 17.2929C17.9342 17.2929 17.8691 17.2799 17.8084 17.2547C17.7476 17.2295 17.6924 17.1926 17.646 17.146L14.354 13.854C14.2602 13.7603 14.1331 13.7076 14.0005 13.7076C13.8679 13.7076 13.7408 13.7603 13.647 13.854L7 20.5M9 23.5V26.5M13 19.5V26.5"
                stroke="white"
                stroke-width="2"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
            </svg>
            Add Market Salary Data
          </span>
        </div>
      </div>
      {error && <p className="error">{error}</p>}
      {success && <p className="success">{success}</p>}

      <div className="b-form">
        <div className="b-label-container">
          <div className="b-position-wrapper">
            <span className="b-label">POSITION TITLE</span>
            <div className="dropdown-wrapper">
              <select
                className="b-form-input-p"
                name="positionId"
                value={positionId}
                onChange={(e) => {
                  setPositionId(e.target.value);
                  setError(null);
                }}
              >
                <option value="">Select Position</option>

                {positions.map((pos) => (
                  <option key={pos.positionId} value={pos.positionId}>
                    {pos.positionTitle}
                  </option>
                ))}
              </select>
              <svg className="dropdown-icon" viewBox="0 0 20 20">
                <path d="M10 13L14 9H6L10 13Z" />
              </svg>
            </div>
          </div>
          <div className="b-date-wrapper">
            <span className="b-label-date">Year</span>
            <div className="dropdown-wrapper-date">
              <select
                className="b-form-date-select"
                name="year"
                value={cards.year}
                onChange={(e) => handleCardChange(loc, "year", e.target.value)}
              >
                <option value="">Select Year</option>

                {YEARS.map((year) => (
                  <option key={year} value={year}>
                    {year}
                  </option>
                ))}
              </select>
              {cardErrors.year && (
                <span className="error">{cardErrors.year}</span>
              )}
            </div>

            <svg className="dropdown-icon" viewBox="0 0 20 20">
              <path d="M10 13L14 9H6L10 13Z" />
            </svg>
          </div>
        </div>
        {/* location cards */}
        {(() => {
          const loc = currentLoc;
          const card = cards[loc];
          const cardErrors = errors[loc];
          const hasConflict = conflicts.includes(loc);

          return (
            <div key={loc} className="b-location-card">
              <div className="b-location-header">
                <div>
                  <span className="b-location-card-title">{loc}</span>
                  <p className="b-location-card-sub">
                    South Africa Market Benchmark
                  </p>
                </div>
                {hasConflict && (
                  <span className="b-conflict-badge">
                    ⚠ Benchmark already exists — edit instead
                  </span>
                )}
              </div>

              <div className="b-step-indicator">
                {LOCATIONS.map((l, i) => (
                  <span
                    key={l}
                    className={`b-step-dot ${i === currentLocation ? "b-step-dot--active" : ""}`}
                  />
                ))}
                <span className="b-step-label">
                  {currentLocation + 1} of {LOCATIONS.length}
                </span>
              </div>

              <div className="b-percentile-wrapper">
                <div className="b-percentile">
                  <div className="b-percentile25">
                    <span className="bp-label">25th Percentile(Lowest)</span>
                    <div className="bp-source-wrapper">
                      <input
                        className="bp-form-input"
                        type="number"
                        min="0"
                        value={card.salary25th}
                        onChange={(e) =>
                          handleCardChange(loc, "salary25th", e.target.value)
                        }
                      />
                      {cardErrors.salary25th && (
                        <span className="error">{cardErrors.salary25th}</span>
                      )}
                    </div>
                  </div>
                  <div className="b-percentile50">
                    <span className="bp-label">50th Percentile(Median)</span>
                    <div className="bp-source-wrapper">
                      <input
                        className="bp-form-input"
                        type="number"
                        min="0"
                        value={card.salary50th}
                        onChange={(e) =>
                          handleCardChange(loc, "salary50th", e.target.value)
                        }
                      />
                      {cardErrors.salary50th && (
                        <span className="error">{cardErrors.salary50th}</span>
                      )}
                    </div>
                  </div>

                  <div className="b-percentile75">
                    <span className="bp-label">75th Percentile(Highest)</span>
                    <div className="bp-source-wrapper">
                      <input
                        className="bp-form-input"
                        type="number"
                        min="0"
                        value={card.salary75th}
                        onChange={(e) =>
                          handleCardChange(loc, "salary75th", e.target.value)
                        }
                      />
                      {cardErrors.salary75th && (
                        <span className="error">{cardErrors.salary75th}</span>
                      )}
                    </div>
                  </div>
                </div>
              </div>

              <div className="b-source-container">
                <div className="b-label-wrapper">
                  <span className="bp-label-s">SOURCE</span>
                </div>
                <div className="bp-source-wrapper">
                  <input
                    className="bp-source-box"
                    type="text"
                    value={card.source}
                    onChange={(e) =>
                      handleCardChange(loc, "source", e.target.value)
                    }
                    placeholder="Payscale Only"
                  />
                  {cardErrors.source && (
                    <span className="error">{cardErrors.source}</span>
                  )}
                </div>
              </div>
            </div>
          );
        })()}
      </div>

      <div className="b-buttons">
        <button className="b-reset" onClick={handleReset}>
          <svg
            width="20"
            height="20"
            viewBox="0 0 20 20"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M2.5 10C2.5 11.4834 2.93987 12.9334 3.76398 14.1668C4.58809 15.4001 5.75943 16.3614 7.12987 16.9291C8.50032 17.4968 10.0083 17.6453 11.4632 17.3559C12.918 17.0665 14.2544 16.3522 15.3033 15.3033C16.3522 14.2544 17.0665 12.918 17.3559 11.4632C17.6453 10.0083 17.4968 8.50032 16.9291 7.12987C16.3614 5.75943 15.4001 4.58809 14.1668 3.76398C12.9334 2.93987 11.4834 2.5 10 2.5C7.90329 2.50789 5.89081 3.32602 4.38333 4.78333L2.5 6.66667M2.5 6.66667V2.5M2.5 6.66667H6.66667"
              stroke="#355867"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
          </svg>
          Reset
        </button>

        {/* back — only show on second card */}
        {currentLocation > 0 && (
          <button
            className="b-reset"
            onClick={() => setCurrentLocation((p) => p - 1)}
          >
            ← Back
          </button>
        )}
        {currentLocation < LOCATIONS.length - 1 ? (
          <button
            className="b-complete"
            onClick={() => setCurrentLocation((p) => p + 1)}
          >
            Next →
          </button>
        ) : (
          <button
            type="button"
            className="b-complete"
            onClick={handleSave}
            disabled={saving}
          >
            <svg
              width="20"
              height="20"
              viewBox="0 0 20 20"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                d="M16.6654 5L7.4987 14.1667L3.33203 10"
                stroke="white"
                stroke-width="2"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
            </svg>
            {saving ? "Saving..." : "Save Benchmarks"}
          </button>
        )}
      </div>
    </div>
  );
}

export default AddBenchmark;

