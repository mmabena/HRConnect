import React, { useEffect, useMemo, useState } from "react";
import PayrollNavbar from "../../Components/PayrollNavBar";
import api from "../../api/api.js";
import "./SalaryBenchmark.css";
import AddBenchmark from "./AddBenchmark.jsx";
import EditBenchmark from "./EditBenchmark.jsx";

function getInitials(name) {
  return name
    .split(" ")
    .map((name) => name.charAt(0))
    .join("")
    .substring(0, 2)
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

//icons(svg) for markets

function IconBelowMarket() {
  return (
    <svg
      width="28"
      height="24"
      viewBox="0 0 28 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        d="M12.5 17H17M17 17V11M17 17L10.625 8.5L6.875 13.5L2 7"
        stroke="#F45052"
        stroke-linecap="round"
        stroke-linejoin="round"
      />
    </svg>
  );
}

function IconAtMarket() {
  return (
    <svg
      width="28"
      height="24"
      viewBox="0 0 28 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        d="M13 7.5H17.5M17.5 7.5V13.5M17.5 7.5L11.125 16L7.375 11L2.5 17.5"
        stroke="#638549"
        stroke-linecap="round"
        stroke-linejoin="round"
      />
    </svg>
  );
}

function IconAboveMarket() {
  return (
    <svg
      width="28"
      height="24"
      viewBox="0 0 28 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        d="M13 7.5H17.5M17.5 7.5V13.5M17.5 7.5L11.125 16L7.375 11L2.5 17.5"
        stroke="#1D8DC5"
        stroke-linecap="round"
        stroke-linejoin="round"
      />
    </svg>
  );
}
//status badge
function StatusBadge({ status }) {
  if (status === "Below Market") {
    return (
      <span className="sb-badge sb-badge--below-market">
        <IconBelowMarket /> Below Market
      </span>
    );
  }

  if (status === "At Market") {
    return (
      <span className="sb-badge sb-badge--at-market">
        <IconAtMarket /> At Market
      </span>
    );
  }

  if (status === "Above Market") {
    return (
      <span className="sb-badge sb-badge--above-market">
        <IconAboveMarket /> Above Market
      </span>
    );
  }

  return <span className="sb-badge sb-badge--nodata">No Data</span>;
}

// ─── avatar ───────────────────────────────────────────────────────────────────
const AVATAR_COLORS = ["blue", "teal", "amber", "coral", "purple"];

function EmployeeAvatar({ name, index }) {
  const color = AVATAR_COLORS[index % AVATAR_COLORS.length];

  return (
    <div className={`sb-avatar sb-avatar--${color}`}>{getInitials(name)}</div>
  );
}

// ─── range bar ────────────────────────────────────────────────────────────────

function RangeBar({ employee }) {
  const { monthlySalary, salary25th, salary50th, salary75th, source } =
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
      <p className="sb-bench-source">{source}</p>

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

      {/* Custom properties are set here so the css can position each
      element. This is a standard pattern, THE VALUES ARE DATA, NOT STYLES.
      All the styles are in the css file associated */}
      <div
        className="sb-range-wrap"
        style={{
          "--p25": `${p25pct}%`,
          "--p50": `${p50pct}%`,
          "--p75": `${p75pct}%`,
          "--sal": `${salpct}%`,
          "--market-width": `${p75pct - p25pct}%`,
          "--above-width": `${100 - p75pct}%`,
        }}
      >
        {/* employee salary label — sits above the dot */}
        <span className="sb-range-emp-label">{fmtRand(monthlySalary)}</span>

        <div className="sb-range-track">
          {/* colour zones */}
          <div className="sb-zone sb-zone--below" />
          <div className="sb-zone sb-zone--market" />
          <div className="sb-zone sb-zone--above" />

          {/* P25 tick */}
          <div className="sb-tick sb-tick--p25">
            <span className="sb-tick-top">P25</span>
            <span className="sb-tick-bottom">{fmtRand(salary25th)}</span>
          </div>

          {/* P50 tick */}
          <div className="sb-tick sb-tick--p50 ">
            <span className="sb-tick-top">P50</span>
            <span className="sb-tick-bottom">{fmtRand(salary50th)}</span>
          </div>

          {/* P75 tick */}
          <div className="sb-tick sb-tick--p75">
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

function EmployeeCard({ employee, index, isOpen, onToggle }) {
  const status = getMarketStatus(employee);

  return (
    <div className="sb-card">
      <div className="sb-card-header" onClick={onToggle}>
        <EmployeeAvatar name={employee.fullName} index={index} />

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
            <div className="sb-card-container">
              <span className="sb-label-ms">
                Monthly Salary:{" "}
                <strong>{fmtRand(employee.monthlySalary)}</strong>
              </span>
            </div>
            <div className="sb-card-container-l">
              <span className="sb-label">
                Location: <strong>{employee.location || "—"}</strong>
              </span>
            </div>
          </div>
          <RangeBar employee={employee} />
        </div>
      )}
    </div>
  );
}

function Pagination({
  currentPage,
  totalPages,
  itemsPerPage,
  onPageChange,
  onItemsPerPageChange,
  totalItems,
}) {
  if (totalPages <= 0) return null;

  function getPages() {
    if (totalPages <= 7) {
      return Array.from({ length: totalPages }, (_, i) => i + 1);
    }
    const pages = [1];
    if (currentPage > 3) pages.push("...");
    for (
      let i = Math.max(2, currentPage - 1);
      i <= Math.min(totalPages - 1, currentPage + 1);
      i++
    ) {
      pages.push(i);
    }
    if (currentPage < totalPages - 2) pages.push("...");
    pages.push(totalPages);
    return pages;
  }

  return (
    <div className="sb-pagination">
      {/* left — X of Y */}
      <div className="sb-pagination-left">
        <span className="sb-page-info">
          <strong>{currentPage}</strong> of {totalPages}
        </span>

        {/* per page dropdown */}
        <div className="sb-perpage-wrapper">
          <select
            className="sb-perpage-select"
            value={itemsPerPage}
            onChange={(e) => {
              onItemsPerPageChange(Number(e.target.value));
              onPageChange(1); // reset to page 1 when per page changes
            }}
          >
            <option value={5}>5</option>
            <option value={10}>10</option>
            <option value={15}>15</option>
            <option value={18}>18</option>
            <option value={20}>20</option>
            

          </select>
          <svg
          className="sb-perpage-icon"
            width="20"
            height="20"
            viewBox="0 0 20 20"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M10 13L14 9H6L10 13ZM10 20C8.61667 20 7.31667 19.7375 6.1 19.2125C4.88333 18.6875 3.825 17.975 2.925 17.075C2.025 16.175 1.3125 15.1167 0.7875 13.9C0.2625 12.6833 0 11.3833 0 10C0 8.61667 0.2625 7.31667 0.7875 6.1C1.3125 4.88333 2.025 3.825 2.925 2.925C3.825 2.025 4.88333 1.3125 6.1 0.7875C7.31667 0.2625 8.61667 0 10 0C11.3833 0 12.6833 0.2625 13.9 0.7875C15.1167 1.3125 16.175 2.025 17.075 2.925C17.975 3.825 18.6875 4.88333 19.2125 6.1C19.7375 7.31667 20 8.61667 20 10C20 11.3833 19.7375 12.6833 19.2125 13.9C18.6875 15.1167 17.975 16.175 17.075 17.075C16.175 17.975 15.1167 18.6875 13.9 19.2125C12.6833 19.7375 11.3833 20 10 20ZM10 18C12.2333 18 14.125 17.225 15.675 15.675C17.225 14.125 18 12.2333 18 10C18 7.76667 17.225 5.875 15.675 4.325C14.125 2.775 12.2333 2 10 2C7.76667 2 5.875 2.775 4.325 4.325C2.775 5.875 2 7.76667 2 10C2 12.2333 2.775 14.125 4.325 15.675C5.875 17.225 7.76667 18 10 18Z"
              fill="#006088"
            />
          </svg>
        </div>
        <span className="sb-perpage-label">Per page</span>
      </div>

      {/* right — page buttons */}
      <div className="sb-pagination-right">
        {/* first page */}
        <button
          className="sb-page-btn"
          onClick={() => onPageChange(1)}
          disabled={currentPage === 1}
          title="First page"
        >
          <svg
            width="20"
            height="20"
            viewBox="0 0 20 20"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M7 10L11 14L11 6L7 10ZM-4.37114e-07 10C-3.76646e-07 8.61667 0.2625 7.31667 0.7875 6.1C1.3125 4.88333 2.025 3.825 2.925 2.925C3.825 2.025 4.88333 1.3125 6.1 0.787499C7.31667 0.262499 8.61667 -4.97581e-07 10 -4.37114e-07C11.3833 -3.76646e-07 12.6833 0.2625 13.9 0.7875C15.1167 1.3125 16.175 2.025 17.075 2.925C17.975 3.825 18.6875 4.88333 19.2125 6.1C19.7375 7.31667 20 8.61667 20 10C20 11.3833 19.7375 12.6833 19.2125 13.9C18.6875 15.1167 17.975 16.175 17.075 17.075C16.175 17.975 15.1167 18.6875 13.9 19.2125C12.6833 19.7375 11.3833 20 10 20C8.61667 20 7.31666 19.7375 6.1 19.2125C4.88333 18.6875 3.825 17.975 2.925 17.075C2.025 16.175 1.3125 15.1167 0.7875 13.9C0.2625 12.6833 -4.97581e-07 11.3833 -4.37114e-07 10ZM2 10C2 12.2333 2.775 14.125 4.325 15.675C5.875 17.225 7.76667 18 10 18C12.2333 18 14.125 17.225 15.675 15.675C17.225 14.125 18 12.2333 18 10C18 7.76667 17.225 5.875 15.675 4.325C14.125 2.775 12.2333 2 10 2C7.76667 2 5.875 2.775 4.325 4.325C2.775 5.875 2 7.76667 2 10Z"
              fill="#123d50"
            />
          </svg>
        </button>

        {/* prev */}
        <button
          className="sb-page-btn"
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage === 1}
          title="Previous page"
        >
          <svg
            width="20"
            height="20"
            viewBox="0 0 20 20"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M7 10L11 14L11 6L7 10ZM-4.37114e-07 10C-3.76646e-07 8.61667 0.2625 7.31667 0.7875 6.1C1.3125 4.88333 2.025 3.825 2.925 2.925C3.825 2.025 4.88333 1.3125 6.1 0.787499C7.31667 0.262499 8.61667 -4.97581e-07 10 -4.37114e-07C11.3833 -3.76646e-07 12.6833 0.2625 13.9 0.7875C15.1167 1.3125 16.175 2.025 17.075 2.925C17.975 3.825 18.6875 4.88333 19.2125 6.1C19.7375 7.31667 20 8.61667 20 10C20 11.3833 19.7375 12.6833 19.2125 13.9C18.6875 15.1167 17.975 16.175 17.075 17.075C16.175 17.975 15.1167 18.6875 13.9 19.2125C12.6833 19.7375 11.3833 20 10 20C8.61667 20 7.31666 19.7375 6.1 19.2125C4.88333 18.6875 3.825 17.975 2.925 17.075C2.025 16.175 1.3125 15.1167 0.7875 13.9C0.2625 12.6833 -4.97581e-07 11.3833 -4.37114e-07 10ZM2 10C2 12.2333 2.775 14.125 4.325 15.675C5.875 17.225 7.76667 18 10 18C12.2333 18 14.125 17.225 15.675 15.675C17.225 14.125 18 12.2333 18 10C18 7.76667 17.225 5.875 15.675 4.325C14.125 2.775 12.2333 2 10 2C7.76667 2 5.875 2.775 4.325 4.325C2.775 5.875 2 7.76667 2 10Z"
              fill="#006088"
            />
          </svg>
        </button>

        {/* page numbers */}
        {getPages().map((page, i) =>
          page === "..." ? (
            <span key={`dots-${i}`} className="sb-page-dots">
              ...
            </span>
          ) : (
            <button
              key={page}
              className={`sb-page-btn ${currentPage === page ? "sb-page-btn--active" : ""}`}
              onClick={() => onPageChange(page)}
            >
              {page}
            </button>
          ),
        )}

        {/* next */}
        <button
          className="sb-page-btn"
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage === totalPages}
          title="Next page"
        >
          <svg
            width="20"
            height="20"
            viewBox="0 0 20 20"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M13 10L9 6L9 14L13 10ZM20 10C20 11.3833 19.7375 12.6833 19.2125 13.9C18.6875 15.1167 17.975 16.175 17.075 17.075C16.175 17.975 15.1167 18.6875 13.9 19.2125C12.6833 19.7375 11.3833 20 10 20C8.61667 20 7.31667 19.7375 6.1 19.2125C4.88333 18.6875 3.825 17.975 2.925 17.075C2.025 16.175 1.3125 15.1167 0.7875 13.9C0.2625 12.6833 1.02753e-07 11.3833 1.19249e-07 10C1.35745e-07 8.61667 0.2625 7.31667 0.7875 6.1C1.3125 4.88333 2.025 3.825 2.925 2.925C3.825 2.025 4.88333 1.3125 6.1 0.7875C7.31667 0.262501 8.61667 1.02753e-07 10 1.19249e-07C11.3833 1.35745e-07 12.6833 0.262501 13.9 0.787501C15.1167 1.3125 16.175 2.025 17.075 2.925C17.975 3.825 18.6875 4.88333 19.2125 6.1C19.7375 7.31667 20 8.61667 20 10ZM18 10C18 7.76667 17.225 5.875 15.675 4.325C14.125 2.775 12.2333 2 10 2C7.76667 2 5.875 2.775 4.325 4.325C2.775 5.875 2 7.76667 2 10C2 12.2333 2.775 14.125 4.325 15.675C5.875 17.225 7.76667 18 10 18C12.2333 18 14.125 17.225 15.675 15.675C17.225 14.125 18 12.2333 18 10Z"
              fill="#006088"
            />
          </svg>
        </button>

        {/* last page */}
        <button
          className="sb-page-btn"
          onClick={() => onPageChange(totalPages)}
          disabled={currentPage === totalPages}
          title="Last page"
        >
          <svg
            width="20"
            height="20"
            viewBox="0 0 20 20"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M13 10L9 6L9 14L13 10ZM20 10C20 11.3833 19.7375 12.6833 19.2125 13.9C18.6875 15.1167 17.975 16.175 17.075 17.075C16.175 17.975 15.1167 18.6875 13.9 19.2125C12.6833 19.7375 11.3833 20 10 20C8.61667 20 7.31667 19.7375 6.1 19.2125C4.88333 18.6875 3.825 17.975 2.925 17.075C2.025 16.175 1.3125 15.1167 0.7875 13.9C0.2625 12.6833 1.02753e-07 11.3833 1.19249e-07 10C1.35745e-07 8.61667 0.2625 7.31667 0.7875 6.1C1.3125 4.88333 2.025 3.825 2.925 2.925C3.825 2.025 4.88333 1.3125 6.1 0.7875C7.31667 0.262501 8.61667 1.02753e-07 10 1.19249e-07C11.3833 1.35745e-07 12.6833 0.262501 13.9 0.787501C15.1167 1.3125 16.175 2.025 17.075 2.925C17.975 3.825 18.6875 4.88333 19.2125 6.1C19.7375 7.31667 20 8.61667 20 10ZM18 10C18 7.76667 17.225 5.875 15.675 4.325C14.125 2.775 12.2333 2 10 2C7.76667 2 5.875 2.775 4.325 4.325C2.775 5.875 2 7.76667 2 10C2 12.2333 2.775 14.125 4.325 15.675C5.875 17.225 7.76667 18 10 18C12.2333 18 14.125 17.225 15.675 15.675C17.225 14.125 18 12.2333 18 10Z"
              fill="#006088"
            />
          </svg>
        </button>
      </div>
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

  const [viewMode, setViewMode] = useState("employees");

  const [editingBenchmark, setEditingBenchmark] = useState(null);

  const reloadBenchmarks = async () => {
    try {
      const res = await api.get("/salary-benchmarks");
      setBenchmark(res.data);
    } catch (err) {
      setError(err.message);
    }
  };

  useEffect(() => {
    reloadBenchmarks();
  }, []);

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

  //  useEffect(() => {
  //     async function load() {
  //       try {
  //         const res = await api.get("/salary-benchmarks");
  //         setBenchmark(res.data);
  //       } catch (err) {
  //         setError(err.message);
  //       } finally {
  //         setLoading(false);
  //       }
  //     }
  //     load();
  //   }, []);

  const handleAddBenchmark = async () => {
    try {
      setLoading(true);
      const res = await api.post("/salary-benchmarks");

      setBenchmark((prev) => [...prev, res.data]);
      setShowPopup(false); // close popup
      // reset form
    
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
      if (viewMode === "benchmarks" && !e.salary25th) return false;

      if (filterPosition && e.positionTitle !== filterPosition) return false;
      if (filterBranch && e.location !== filterBranch) return false;
      if (filterStatus && getMarketStatus(e) !== filterStatus) return false;
      return true;
    });
  }, [employees, viewMode, filterPosition, filterBranch, filterStatus]);

  const benchmarkEmployees = useMemo(
    () => employees.filter((e) => e.salary25th),
    [employees],
  );

  // ── employees view stats (based on filtered employees)
  const totalBenchmarks = useMemo(
    () => filtered.filter((e) => e.salary25th).length,
    [filtered],
  );

  const positionsCovered = useMemo(
    () =>
      new Set(filtered.filter((e) => e.salary25th).map((e) => e.positionTitle))
        .size,
    [filtered],
  );

  const locations = useMemo(
    () => new Set(filtered.map((e) => e.location).filter(Boolean)).size,
    [filtered],
  );

  // ── benchmarks view stats (based on the benchmark table)
  const totalBenchmarkEntries = useMemo(() => benchmark.length, [benchmark]);

  const benchmarkPositionsCovered = useMemo(
    () => new Set(benchmark.map((b) => b.positionTitle)).size,
    [benchmark],
  );

  const benchmarkLocations = useMemo(
    () => new Set(benchmark.map((b) => b.location).filter(Boolean)).size,
    [benchmark],
  );

  const [empPage, setEmpPage] = useState(1);
  const [empPerPage, setEmpPerPage] = useState(5);
  const [benchmarkPage, setBenchmarkPage] = useState(1);

  const [benchmarkPerPage, setBenchmarkPerPage] = useState(5);

  useEffect(() => {
    setEmpPage(1);
  }, [filterPosition, filterStatus, filterBranch]);

  const totalEmpPages = Math.ceil(filtered.length / empPerPage);

  const paginatedEmployees = useMemo(() => {
    const start = (empPage - 1) * empPerPage;
    return filtered.slice(start, start + empPerPage);
  }, [filtered, empPage, empPerPage]);

  const totalBenchmarkPages = Math.ceil(benchmark.length / benchmarkPerPage);
  const paginatedBenchmarks = useMemo(() => {
    const start = (benchmarkPage - 1) * benchmarkPerPage;
    return benchmark.slice(start, start + benchmarkPerPage);
  }, [benchmark, benchmarkPage, benchmarkPerPage]);

  function toggleCard(id) {
    setOpenId((prev) => (prev === id ? null : id));
  }

  return (
    <div className="menu-background custom-scrollbar">
      <div className="sb-wrap-container">
        <div className="sb-container">Payroll Management </div>
        <div className="sb-actions">
          <button className="sb-btn" onClick={() => setViewMode("employees")}>
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
          <button className="sb-btn" onClick={() => setViewMode("benchmarks")}>
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
          {viewMode === "employees" ? (
            <>
              <div className="sb-stat">
                <p className="sb-stat-label">Total Benchmarks</p>
                <p className="sb-stat-value">
                  {loading ? "-" : totalBenchmarks}
                </p>
              </div>
              <div className="sb-stat">
                <p className="sb-stat-label">Positions Covered</p>
                <p className="sb-stat-value">
                  {loading ? "-" : positionsCovered}
                </p>
              </div>
              <div className="sb-stat">
                <p className="sb-stat-label">Locations</p>
                <p className="sb-stat-value">{loading ? "—" : locations}</p>
              </div>
            </>
          ) : (
            <>
              <div className="sb-stat">
                <p className="sb-stat-label">Total Benchmarks</p>
                <p className="sb-stat-value">
                  {loading ? "—" : totalBenchmarkEntries}
                </p>
              </div>
              <div className="sb-stat">
                <p className="sb-stat-label">Positions Covered</p>
                <p className="sb-stat-value">
                  {loading ? "—" : benchmarkPositionsCovered}
                </p>
              </div>
              <div className="sb-stat">
                <p className="sb-stat-label">Locations</p>
                <p className="sb-stat-value">
                  {loading ? "—" : benchmarkLocations}
                </p>
              </div>
            </>
          )}
        </div>
      </div>

      {viewMode === "employees" && (
        <>
          {loading && <p className="sb-state-msg">Loading Employees</p>}
          {error && (
            <p className="sb-state-msg sb-state-msg--error">Error: {error}</p>
          )}

          {!loading && !error && (
            <>
              <div className="sb-cards">
                {filtered.length === 0 ? (
                  <p className="sb-state-msg">
                    No employees match the filters selected
                  </p>
                ) : (
                  paginatedEmployees.map((emp, i) => (
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

              <Pagination
                currentPage={empPage}
                totalPages={totalEmpPages}
                itemsPerPage={empPerPage}
                onPageChange={setEmpPage}
                onItemsPerPageChange={(val) => {
                  setEmpPerPage(val);
                  setEmpPage(1);
                }}
              />
            </>
          )}

          {showPopup && (
            <div className="modal-overlay">
              <AddBenchmark
                onClose={() => setShowPopup(false)}
                onAddSuccess={async (newBenchmark) => {
                  await reloadBenchmarks();
                  const res = await api.get("/salary-benchmarks/employees");
                  setEmployees(res.data);
                  setShowPopup(false);
                }}
              />
            </div>
          )}
        </>
      )}

      {viewMode === "benchmarks" && (
        <>
          {/* ✅ modal lives outside the table */}
          {editingBenchmark && (
            <div className="modal-overlay">
              <EditBenchmark
                benchmark={editingBenchmark}
                onClose={() => setEditingBenchmark(null)}
                onEditSuccess={(updated) => {
                  setBenchmark((prev) =>
                    prev.map((b) => (b.id === updated.id ? updated : b)),
                  );
                  setEditingBenchmark(null);
                }}
              />
            </div>
          )}

          <div className="sb-table-wrap">
            <div className="sb-table-container">
              <table className="sb-table">
                <thead>
                  <tr>
                    <th>Position</th>
                    <th>Location</th>
                    <th>P25</th>
                    <th>P50</th>
                    <th>P75</th>
                    <th>Source</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {paginatedBenchmarks.map((b) => (
                    <tr key={b.id}>
                      <td>{b.positionTitle || b.positionId}</td>
                      <td>{b.location}</td>
                      <td>R {b.salary25th.toLocaleString("en-ZA")}</td>
                      <td>R {b.salary50th.toLocaleString("en-ZA")}</td>
                      <td>R {b.salary75th.toLocaleString("en-ZA")}</td>
                      <td>{b.source}</td>
                      <td className="sb-table-actions">
                        <button
                          className="sb-table-btn sb-table-btn--edit"
                          onClick={() => setEditingBenchmark(b)}
                        >
                          Edit
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          <Pagination
            currentPage={benchmarkPage}
            totalPages={totalBenchmarkPages}
            itemsPerPage={benchmarkPerPage}
            onPageChange={setBenchmarkPage}
            onItemsPerPageChange={(val) => {
              setBenchmarkPerPage(val);
              setBenchmarkPage(1);
            }}
          />

          {showPopup && (
            <div className="modal-overlay">
              <AddBenchmark
                onClose={() => setShowPopup(false)}
                onAddSuccess={async () => {
                  await reloadBenchmarks();
                  const res = await api.get("/salary-benchmarks/employees");
                  setEmployees(res.data);
                  setShowPopup(false);
                }}
              />
            </div>
          )}
        </>
      )}
    </div>
  );
}

export default SalaryBenchmark;
