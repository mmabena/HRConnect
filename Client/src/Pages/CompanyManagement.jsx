import React, { useEffect, useState } from "react";
import "./CompanyManagement.css";
import { useNavigate } from "react-router-dom";
import api from "../api/api.js";
import { fetchMyCompanies, switchCompany } from "../api/UserCompany";
import { jwtDecode } from "jwt-decode";
import { ArrowRight } from 'lucide-react';

const getCurrentUser = async () => {
  try {
    const res = await api.get("/user/me");
    return res.data;
  } catch (err) {
    console.error("Failed to decode user:", err);
    return null;
  }
};

const CompanyManagement = () => {
  const [user, setUser] = useState(null);
  const [companies, setCompanies] = useState([]);
  const [selectedCompany, setSelectedCompany] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const data = await fetchMyCompanies();

        console.log("API RESPONSE:", data);

        const list = data?.companies ?? data ?? [];

        const mappedCompanies = Array.isArray(list)
          ? list.map((uc) => ({
              id: uc.companyId,
              name: uc.companyName,
              employeeCount: uc.employeeCount,
              isDefault: uc.isDefault,
              isOriginalCompany: uc.isOriginalCompany,
            }))
          : [];

        setCompanies(mappedCompanies);

        const defaultCompany = mappedCompanies.find((c) => c.isDefault);

        if (defaultCompany) {
          setSelectedCompany(defaultCompany.id);
        }
      } catch (error) {
        console.error("Error fetching data:", error);
        setCompanies([]);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  useEffect(() => {
    const loadUser = async () => {
      const data = await getCurrentUser();
      setUser(data);
    };

    loadUser();
  }, []);

  const displayName = user?.fullName || user?.email || "User";

  const initials = displayName
    .split(" ")
    .map((n) => n.charAt(0))
    .join("")
    .substring(0, 2)
    .toUpperCase();

  const navigate = useNavigate();

  const handleEnterDashboard = async () => {
    if (!selectedCompany) return;

    try {
      await switchCompany(selectedCompany);
      navigate("/dashboard");
    } catch (error) {
      console.error("Error setting active company:", error);
    }
  };

  if (loading) return <div className="loading">Loading...</div>;

  return (
    <div className="comp-page-container">
      {/* Top Header */}
      <div className="comp-top-bar">
        {/* Logo */}
        <div className="comp-logo-frame-46854">
          <span className="comp-logo-singular">Singular</span>
          <span className="comp-logo-express">Express</span>
        </div>
        {/* User Info */}
        {user && (
          <div className="comp-user-container">
            <div className="comp-user-inner">
              <div className="comp-user-avatar">
                <span className="comp-user-initials">{initials}</span>
              </div>

              {/* Name */}
              <span className="comp-user-name-surname">{displayName}</span>

              {/* Dot */}
              <span className="comp-user-dot">•</span>

              {/* Position */}
              <span className="comp-user-position">{user?.jobTitle}</span>
            </div>
          </div>
        )}
      </div>

      {/* Main */}
      <div className="comp-content-container">
        <h1 className="comp-company-title">Select a Company</h1>

        <div className="comp-company-list">
          {companies.map((company) => (
            <div
              key={company.id}
              className={`comp-company-card ${
                selectedCompany === company.id ? "active" : ""
              }`}
              onClick={() => setSelectedCompany(company.id)}
            >
              <div className="comp-company-left">
                <div className="comp-company-icon">{getInitials(company.name)}</div>

                <div className="comp-company-info">
                  <div className="comp-company-name">{company.name}</div>
                  <div className="comp-company-employees">
                    {company.employeeCount} Employees
                  </div>
                </div>
              </div>

              {company.isOriginalCompany && (
                <div className="comp-company-default">Default</div>
              )}

              <div className="comp-company-arrow">
                <ArrowRight size={30} className="comp-arrow"/>
              </div>
            </div>
          ))}
        </div>

        <button
          className="comp-enter-dashboard-btn"
          disabled={!selectedCompany}
          onClick={handleEnterDashboard}
        >
          <span className="comp-enter-text">Enter Dashboard</span>
          <span className="comp-enter-icon">
            <ArrowRight size={30} className="comp-arrow"/>
          </span>
        </button>
      </div>

      {/* Footer */}
      <div className="comp-footer-container">
        <span className="comp-footer-text">
          Copyright © 2026 Singular Systems. All rights reserved.
        </span>
      </div>
    </div>
  );
};

// Utility
const getInitials = (name) => {
  if (!name) return "";
  const words = name.split(" ");
  return words.length === 1 ? words[0][0] : words[0][0] + words[1][0];
};

export default CompanyManagement;
