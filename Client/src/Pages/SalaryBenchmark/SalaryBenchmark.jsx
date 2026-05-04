import React, { useEffect, useMemo, useState } from "react";
import PayrollNavbar from "../../Components/PayrollNavBar";
import api from "../../api/api.js";
import "./SalaryBenchmark.css";
import AddBenchmark from "./AddBenchmark.jsx";

function getInitials(name) {
  return name
    .split(" ")
    .map((n) => n[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();
}

function fmtRand(amount) {
  return "R " + Math.round(amount).toLocaleString("en-ZA");
}

function getMarketStatus(employee) {
  const { monthlySalary, salary25th, salary75th } = employee;
  if (!salary25th || !salary75th) return "No Data";
  if (monthlySalary < salary25th) return "Below Market";
  if (monthlySalary <= salary75th) return "At Market";
  return "Above Market";
}

// ─── avatar ───────────────────────────────────────────────────────────────────

function EmployeeAvatar({ name, profileImage }) {
  const [imgError, setImgError] = useState(false);

  if (profileImage && !imgError) {
    return (
      <img
        src={profileImage}
        alt={name}
        className="sb-avatar"
        onError={() => setImgError(true)}
      />
    );
  }

  return (
    <div className="sb-avatar sb-avatar--initials">{getInitials(name)}</div>
  );
}

// ─── range bar ────────────────────────────────────────────────────────────────

function RangeBar({ employee }) {
  const { monthlySalary, salary25th, salary50th, salary75th, benchmarkSource } =
    employee;

  if (!salary25th || !salary50th || !salary75th) {
    return (
      <p className="sb-nodata">
        No benchmark data entered for this position yet.
      </p>
    );
  }

  // calculate where everything sits as a percentage along the bar
  const min = Math.min(monthlySalary, salary25th) * 0.85;
  const max = Math.max(monthlySalary, salary75th) * 1.12;
  const range = max - min;
  const pct = (v) => Math.max(0, Math.min(100, ((v - min) / range) * 100));

  const p25pct = pct(salary25th);
  const p50pct = pct(salary50th);
  const p75pct = pct(salary75th);
  const salpct = pct(monthlySalary);

  const vsMedian = monthlySalary - salary50th;
  const vsFormatted = (vsMedian >= 0 ? "+" : "-") + fmtRand(Math.abs(vsMedian));

  return (
    <div className="sb-bench-box">
      <p className="sb-bench-label">Market Benchmark</p>
      <p className="sb-bench-source">{benchmarkSource}</p>

      {/* legend */}
      <div className="sb-legend">
        <span className="sb-legend-item sb-legend-item--below">
          <svg
            width="30"
            height="47"
            viewBox="0 0 30 47"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <rect
              y="16"
              width="30"
              height="15"
              rx="5"
              fill="#D12C2C"
              fill-opacity="0.3"
            />
          </svg>
          Below P25
        </span>
        <span className="sb-legend-item sb-legend-item--market">
          <svg
            width="30"
            height="15"
            viewBox="0 0 30 15"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <rect width="30" height="15" rx="5" fill="#DDE4C5" />
          </svg>
          Market Range (P25–P75)
        </span>
        <span className="sb-legend-item sb-legend-item--above">
          <svg
            width="30"
            height="15"
            viewBox="0 0 30 15"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <rect width="30" height="15" rx="5" fill="#DDE8EF" />
          </svg>
          Above P75 - Median (P50)
        </span>
      </div>

      {/* range bar */}
      <div className="sb-range-wrap">
        {/* employee salary label — sits above the dot */}
        <span className="sb-emp-label">{fmtRand(monthlySalary)}</span>

        <div className="sb-range-track">
          {/* colour zones */}
          <div className="sb-zone sb-zone--below" />
          <div className="sb-zone sb-zone--market" />
          <div className="sb-zone sb-zone--above" />

          {/* P25 tick */}
          <div className="sb-tick">
            <span className="sb-tick-top">P25</span>
            <span className="sb-tick-bottom">{fmtRand(salary25th)}</span>
          </div>

          {/* P50 tick */}
          <div className="sb-tick sb-tick--median">
            <span className="sb-tick-top">P50</span>
            <span className="sb-tick-bottom">{fmtRand(salary50th)}</span>
          </div>

          {/* P75 tick */}
          <div className="sb-tick">
            <span className="sb-tick-top">P75</span>
            <span className="sb-tick-bottom">{fmtRand(salary75th)}</span>
          </div>

          {/* employee dot marker */}
          <div className="sb-marker" />
        </div>
      </div>

      {/* summary boxes */}
      <div className="sb-pboxes">
        <div className="sb-pbox">
          <p className="sb-pbox-label">P25 (Lowest)</p>
          <p className="sb-pbox-value">{fmtRand(salary25th)}</p>
        </div>
        <div className="sb-pbox">
          <p className="sb-pbox-label">P50 (Median)</p>
          <p className="sb-pbox-value">{fmtRand(salary50th)} </p>
        </div>
        <div className="sb-pbox">
          <p className="sb-pbox-label">P75 (Highest)</p>
          <p className="sb-pbox-value">{fmtRand(salary75th)}</p>
        </div>
        <div className="sb-pbox">
          <p className="sb-pbox-label">vs Median</p>
          <p
            className={`sb-pbox-value ${vsMedian < 0 ? "sb-pbox-value--negative" : ""}`}
          >
            {vsFormatted}
          </p>
        </div>
      </div>
    </div>
  );
}

// ─── employee card

function EmployeeCard({ employee, isOpen, onToggle }) {
  const status = getMarketStatus(employee);

  return (
    <div className="sb-card">
      <div className="sb-card-header" onClick={onToggle}>
        <EmployeeAvatar
          name={employee.fullName}
          profileImage={employee.profileImage}
        />

        <div className="sb-card-info">
          <p className="sb-card-name">{employee.fullName}</p>
          <p className="sb-card-position">{employee.positionTitle}</p>
        </div>

        <div className="sb-card-right">
          <div className="sb-card-salary">
            <p className="sb-card-salary-label">Current Salary</p>
            <p className="sb-card-salary-value">
              {fmtRand(employee.monthlySalary)}
            </p>
          </div>
          <span className="sb-badge sb-badge--location">
            <svg
              width="16"
              height="16"
              viewBox="0 0 16 16"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                d="M13 6.80006C13 9.7959 9.53812 12.9159 8.37562 13.8795C8.26733 13.9577 8.1355 14 8 14C7.8645 14 7.73267 13.9577 7.62438 13.8795C6.46188 12.9159 3 9.7959 3 6.80006C3 5.527 3.52678 4.30609 4.46447 3.40591C5.40215 2.50572 6.67392 2 8 2C9.32608 2 10.5979 2.50572 11.5355 3.40591C12.4732 4.30609 13 5.527 13 6.80006Z"
                stroke="#355867"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
              <path
                d="M8 8.60008C9.03553 8.60008 9.875 7.79418 9.875 6.80006C9.875 5.80593 9.03553 5.00004 8 5.00004C6.96447 5.00004 6.125 5.80593 6.125 6.80006C6.125 7.79418 6.96447 8.60008 8 8.60008Z"
                stroke="#355867"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
            </svg>

            {employee.location || "—"}
          </span>
          <span
            className={`sb-badge sb-badge--${status.toLowerCase().replace(" ", "-")}`}
          >
            {status}
          </span>
          <span className={`sb-chevron ${isOpen ? "sb-chevron--open" : ""}`}>
            &#8963;
          </span>
        </div>
      </div>

      {/* expandable body */}
      {isOpen && (
        <div className="sb-card-body">
          <div className="sb-card-meta">
            <span className="sb-label">
              Annual Salary: <strong>{fmtRand(employee.monthlySalary)}</strong>
            </span>
            <span className="sb-label">
              Location: <strong>{employee.location || "—"}</strong>
            </span>
          </div>
          <RangeBar employee={employee} />
        </div>
      )}
    </div>
  );
}

function SalaryBenchmark() {
  const [employees, setEmployees] = useState([]);
  const [benchmark, setBenchmark] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [openId, setOpenId] = useState(null);
  const [filterPosition, setFilterPosition] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [filterBranch, setFilterBranch] = useState("");
  const [showPopup, setShowPopup] = useState(false);
  const [formData, setFormData] = useState({
    positionTitle: "",
    location: "",
    source: "",
    salary25th: "",
    salary50th: "",
    salary75th: "",
  });

  useEffect(() => {
    async function load() {
      try {
        const res = await api.get("/salary-benchmarks/employees");
        setEmployees(res.data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  useEffect(() => {
    async function load() {
      try {
        const res = await api.get("/salary-benchmarks");
        setBenchmark(res.data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  const handleAddBenchmark = async () => {
    try {
      setLoading(true);
      const res = await api.post("/salary-benchmarks", formData);

      setBenchmark((prev) => [...prev, res.data]);
      setShowPopup(false); // close popup
      // reset form
      setFormData({
        positonTitle: "",
        location: "",
        source: "",
        salary25th: "",
        salary50th: "",
        salary75th: "",
      });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const positions = useMemo(
    () => [...new Set(employees.map((e) => e.positionTitle))].sort(),
    [employees],
  );
  const branches = useMemo(
    () => [...new Set(employees.map((e) => e.location).filter(Boolean))].sort(),
    [employees],
  );

  const filtered = useMemo(() => {
    return employees.filter((e) => {
      if (filterPosition && e.positionTitle !== filterPosition) return false;
      if (filterBranch && e.location !== filterBranch) return false;
      if (filterStatus && getMarketStatus(e) !== filterStatus) return false;
      return true;
    });
  }, [employees, filterPosition, filterBranch, filterStatus]);

  const totalBenchmarks = filtered.filter((e) => e.salary25th).length;
  const positionsCovered = new Set(
    filtered.filter((e) => e.salary25th).map((e) => e.positionTitle),
  ).size;
  const locations = new Set(filtered.map((e) => e.location).filter(Boolean))
    .size;

  function toggleCard(id) {
    setOpenId((prev) => (prev === id ? null : id));
  }

  return (
    <div className="menu-background custom-scrollbar">
      <div className="sb-wrap-container">
        <div className="sb-container">Payroll Management </div>
        <div className="sb-actions">
          <button className="sb-btn">
            <svg
              width="22"
              height="22"
              viewBox="0 0 22 22"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                d="M12.3737 7.3335H9.6237M13.7487 1.8335L12.832 3.66683H15.582C16.0683 3.66683 16.5346 3.85998 16.8784 4.2038C17.2222 4.54762 17.4154 5.01393 17.4154 5.50016V18.3335C17.4154 18.8197 17.2222 19.286 16.8784 19.6299C16.5346 19.9737 16.0683 20.1668 15.582 20.1668H6.41536C5.92913 20.1668 5.46282 19.9737 5.119 19.6299C4.77519 19.286 4.58203 18.8197 4.58203 18.3335V5.50016C4.58203 5.01393 4.77519 4.54762 5.119 4.2038C5.46282 3.85998 5.92913 3.66683 6.41536 3.66683H9.16536M15.4894 20.1668C15.2786 19.1312 14.7164 18.2003 13.8979 17.5317C13.0795 16.8631 12.0551 16.4978 10.9982 16.4978C9.94139 16.4978 8.91702 16.8631 8.09856 17.5317C7.2801 18.2003 6.71786 19.1312 6.50703 20.1668M8.2487 1.8335L10.9987 7.3335M13.7487 13.7502C13.7487 15.2689 12.5175 16.5002 10.9987 16.5002C9.47991 16.5002 8.2487 15.2689 8.2487 13.7502C8.2487 12.2314 9.47991 11.0002 10.9987 11.0002C12.5175 11.0002 13.7487 12.2314 13.7487 13.7502Z"
                stroke="#355867"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
            </svg>
            Employee list
          </button>
          <button className="sb-btn">
            <svg
              width="22"
              height="22"
              viewBox="0 0 22 22"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                d="M10.9987 14.6667V19.25M14.6654 12.8333V19.25M18.332 9.16667V19.25M20.1654 2.75L12.2399 10.6755C12.1973 10.7182 12.1467 10.752 12.091 10.7752C12.0353 10.7983 11.9757 10.8102 11.9154 10.8102C11.8551 10.8102 11.7954 10.7983 11.7397 10.7752C11.684 10.752 11.6334 10.7182 11.5909 10.6755L8.5732 7.65783C8.48725 7.57191 8.37069 7.52364 8.24916 7.52364C8.12762 7.52364 8.01107 7.57191 7.92512 7.65783L1.83203 13.75M3.66536 16.5V19.25M7.33203 12.8333V19.25"
                stroke="#355867"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
            </svg>
            Benchmarks
          </button>
          <button
            className="sb-btn sb-btn--primary"
            onClick={() => setShowPopup(true)}
          >
            <svg
              width="24"
              height="24"
              viewBox="0 0 24 24"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                d="M5 12H19M12 5V19"
                stroke="white"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
            </svg>
            Add new benchmark
          </button>
        </div>
        <div />
      </div>
      <div className="navbar-with-button">
        <PayrollNavbar />
      </div>

      <div className="sb-filters">
        <div className="sb-filters-group">
          <div className="sb-group-filters">
            <span className="sb-filter-label">Position title</span>
            <select
              value={filterPosition}
              onChange={(e) => setFilterPosition(e.target.value)}
            >
              <option value="">All positions</option>
              {positions.map((p) => (
                <option key={p} value={p}>
                  {p}
                </option>
              ))}
            </select>
          </div>

          <div className="sb-group-filters-s">
            <span className="sb-filter-label-s">Salary benchmark</span>
            <select
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
            >
              <option value="">All</option>
              <option value="Below Market">Below market</option>
              <option value="At Market">At market</option>
              <option value="Above Market">Above market</option>
              <option value="No Data">No data</option>
            </select>
          </div>

          <div className="sb-group-filter">
            <div className="sb-filter-branch">
              <span className="sb-filter-label-b">Branch</span>
              <select
                value={filterBranch}
                onChange={(e) => setFilterBranch(e.target.value)}
              >
                <option value="">All branches</option>
                {branches.map((b) => (
                  <option key={b} value={b}>
                    {b}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </div>
      </div>
      <div className="sb-stats">
        <div className="sb-stats-container">
          <div className="sb-stat">
            <p className="sb-stat-label">Total Benchmarks</p>
            <p className="sb-stat-value">{loading ? "-" : totalBenchmarks}</p>
          </div>
          <div className="sb-stat">
            <p className="sb-stat-label">Positions Covered</p>
            <p className="sb-stat-value">{loading ? "-" : positionsCovered}</p>
          </div>
          <div className="sb-stat">
            <p className="sb-stat-label">Locations</p>
            <p className="sb-stat-value">{loading ? "—" : locations}</p>
          </div>
        </div>
      </div>
      {loading && <p className="sb-state-msg">Loading Employees</p>}
      {error && (
        <p className="sb-state-msg sb-state-msg--error">Error:{error}</p>
      )}

      {!loading && !error && (
        <div className="sb-cards">
          {filtered.length === 0 ? (
            <p className="sb-state-msg">
              No employees match the filters selected
            </p>
          ) : (
            filtered.map((emp, i) => (
              <EmployeeCard
                key={emp.employeeId}
                employee={emp}
                index={i}
                isOpen={openId === emp.employeeId}
                onToggle={() => toggleCard(emp.employeeId)}
              />
            ))
          )}
        </div>
      )}
      {showPopup && (
        <div className="modal-overlay">
          <AddBenchmark
            onClose={() => setShowPopup(false)}
            onUploadSuccess={() => {
              handleAddBenchmark();
              setShowPopup(false);
            }}
          />
        </div>
      )}
    </div>
  );
}

export default SalaryBenchmark;
