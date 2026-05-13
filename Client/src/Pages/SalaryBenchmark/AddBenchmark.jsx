import { useState, useEffect } from "react";
import React from "react";
import api from "../../api/api";
import "./AddBenchmark.css";

function AddBenchmark({ onClose, onAddSuccess }) {
  const [positions, setPositions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [errors, setErrors] = useState({});
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [formData, setFormData] = useState({
    positionId: "",
    location: "",
    source: "",
    salary25th: "",
    salary50th: "",
    salary75th: "",
  });
  const [loadingPositions, setLoadingPositions] = useState(true);
  const [saving, setSaving] = useState(false);
  const validate = () => {
    const newErrors = {};
    if (!formData.positionId) {
      newErrors.positionId = "Position is required";
    }

    if (!formData.location.trim()) {
      newErrors.location = "Location is required";
    }

    if (!formData.source.trim()) {
      newErrors.source = "Source is required";
    }

    if (!formData.salary25th) {
      newErrors.salary25th = "25th percentile is required";
    } else if (Number(formData.salary25th) <= 0) {
      newErrors.salary25th = "Must be greater than 0";
    }

    if (!formData.salary50th) {
      newErrors.salary50th = "50th percentile is required";
    } else if (Number(formData.salary50th) <= 0) {
      newErrors.salary50th = "Must be greater than 0";
    }

    if (!formData.salary75th) {
      newErrors.salary75th = "75th percentile is required";
    } else if (Number(formData.salary75th) <= 0) {
      newErrors.salary75th = "Must be greater than 0";
    }

    if (Number(formData.salary25th) >= Number(formData.salary50th)) {
      newErrors.salary50th =
        "50th percentile must be greater than 25th percentile";
    }

    if (Number(formData.salary50th) >= Number(formData.salary75th)) {
      newErrors.salary75th =
        "75th percentile must be greater than 50th percentile";
    }

    setErrors(newErrors);

    return Object.keys(newErrors).length === 0;
  };

  const locations = ["Johannesburg", "Cape Town"];
  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
    setErrors((prev) => ({
    ...prev,
    [e.target.name]: "",
  }));
  };

  const handleReset = () => {
    setFormData({
      positionId: "",
      location: "",
      source: "",
      salary25th: "",
      salary50th: "",
      salary75th: "",
    });

    setErrors({});
    setError(null);
  };

  console.log("SENDING:", formData);

  const handleAddBenchmark = async () => {
    if (saving) return;
    if (!validate()) return;

    try {
      setSaving(true);

      const payload = {
        PositionId: Number(formData.positionId),
        Location: formData.location,
        Source: formData.source,
        Salary25th: Number(formData.salary25th),
        Salary50th: Number(formData.salary50th),
        Salary75th: Number(formData.salary75th),
      };

      console.log("FINAL PAYLOAD:", payload);

      const res = await api.post("/salary-benchmarks", payload);

      console.log("SUCCESS:", res.data);

      onAddSuccess?.(res.data);
      setSuccess("Benchmark saved successfully!");
      handleReset();
    } catch (err) {
      console.log("ERROR:", err);
      setError("Failed to save benchmark");
    } finally {
      setSaving(false);
    }
  };

  useEffect(() => {
    if (success) {
      const timer = setTimeout(() => setSuccess(null), 3000);
      return () => clearTimeout(timer);
    }
  }, [success]);

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
                className="b-form-input"
                name="positionId"
                value={formData.positionId}
                onChange={handleChange}
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
            {errors.positionId && (
              <span className="error">{errors.positionId}</span>
            )}
          </div>
          <div className="b-location-wrapper">
            <span className="b-label">LOCATION</span>
            <div className="dropdown-wrapper">
              <select
                className="b-form-input"
                name="location"
                value={formData.location}
                onChange={handleChange}
              >
                <option value="">Select location</option>
                {locations.map((l, index) => (
                  <option key={index} value={l}>
                    {l}
                  </option>
                ))}
              </select>

              <svg className="dropdown-icon" viewBox="0 0 20 20">
                <path d="M10 13L14 9H6L10 13Z" />
              </svg>
            </div>
            {errors.location && (
  <span className="error">{errors.location}</span>
)}
          </div>
        </div>
        <div className="b-percentile-container">
          <span className="b-salarypercentile">Base Salary Percentiles</span>
        </div>
        <div className="b-percentile-wrapper">
          <div className="b-percentile">
            <div className="b-percentile25">
              <span className="bp-label">25th Percentile(Lowest)</span>
              <input
                className="bp-form-input"
                type="number"
                min="0"
                name="salary25th"
                value={formData.salary25th}
                onChange={handleChange}
              />
              {errors.salary25th && (
  <span className="error">{errors.salary25th}</span>
)}
            </div>
            <div className="b-percentile50">
              <span className="bp-label">50th Percentile(Median)</span>
              <input
                className="bp-form-input"
                type="number"
                min="0"
                name="salary50th"
                value={formData.salary50th}
                onChange={handleChange}
              />
              {errors.salary50th && (
  <span className="error">{errors.salary50th}</span>
)}
            </div>

            <div className="b-percentile75">
              <span className="bp-label">75th Percentile(Highest)</span>

              <input
                className="bp-form-input"
                type="number"
                min="0"
                name="salary75th"
                value={formData.salary75th}
                onChange={handleChange}
              />
              {errors.salary75th && (
  <span className="error">{errors.salary75th}</span>
)}
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
              name="source"
              value={formData.source}
              onChange={handleChange}
              placeholder="Payscale Only"
            />
            {errors.source && (
  <span className="error">{errors.source}</span>
)}
          </div>
        </div>
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
        <button
          type="button"
          className="b-complete"
          onClick={handleAddBenchmark}
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
          {loading ? "Save Benchmark" : "Saving ..."}
        </button>
      </div>
    </div>
  );
}

export default AddBenchmark;
