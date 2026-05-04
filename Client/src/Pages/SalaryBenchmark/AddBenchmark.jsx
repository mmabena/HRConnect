import { useState, useEffect } from "react";
import React from "react";
import api from "../../api/api";
import "./AddBenchmark.css";

function AddBenchmark({ onClose, onAddSuccess }) {
  const [positions, setPositions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [formData, setFormData] = useState({
    positionTitle: "",
    location: "",
    source: "",
    salary25th: "",
    salary50th: "",
    salary75th: "",
  });

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleAddBenchmark = async () => {
    if (
      !formData.positionTitle ||
      !formData.location ||
      !formData.source ||
      !formData.salary25th ||
      !formData.salary50th ||
      !formData.salary75th
    ) {
      setError("Please fill required fields");
      return;
    }
    try {
      setLoading(true);
      const res = await api.post("/salary-benchmarks", formData);
      onAddSuccess(res.data);
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    async function loadPositions() {
      try {
        const res = await api.get("/positions");
        console.log("RAW POSITIONS RESPONSE:", res.data);
        setPositions(res.data);
      } catch (err) {
        console.error(err);
      }
    }

    loadPositions();
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

      <div className="b-form">
        <div className="b-label-container">
          <span className="b-label">Position Title</span>
          <div className="dropdown-wrapper">
            <select
              className="b-form-input"
              name="positionTitle"
              value={formData.positionTitle}
              onChange={handleChange}
            >
              <option value="">Select Position</option>

              {positions.map((pos) => (
                <option key={pos.id} value={pos.positionTitle}>
                  {pos.positionTitle}
                </option>
              ))}
              
            </select>

            <svg className="dropdown-icon" viewBox="0 0 20 20">
              <path d="M10 13L14 9H6L10 13Z" />
            </svg>
          </div>
        </div>
      </div>
      <div className="b-buttons">
        <button className="b-reset">
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
        <button className="b-complete">
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
          Save Benchmark
        </button>
      </div>
    </div>
  );
}

export default AddBenchmark;
