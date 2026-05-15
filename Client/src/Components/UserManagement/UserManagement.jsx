import React, { useState, useEffect } from "react";
import RolesModal from "./RoleModal.jsx"; // Import the new RolesModal
import { fetchUsersAndRoles, updateUserRole } from "../../api/UserManagement";
import { fetchAllEmployees } from '../../api/Employee.js'
import { getStoredUserRole } from "../../utils/roleUtils";
import useInitialColors from "../../hooks/useInitialColors";
import { resolveRole } from "../../utils/roleUtils.js";
import { SlidersHorizontal, SearchIcon } from "lucide-react"
import {toast} from "react-toastify";
import {
  FaUser,
  FaUsers,
  FaUserLock,
  FaCheckCircle,
  FaTimesCircle,
  FaEdit,
  FaEllipsisV,
  FaTimes,
} from "react-icons/fa";
import "./UserManagement.css";
import usePagination from "../../hooks/useEmpPagination.js";
import useDropdown from "../../hooks/useDropdown";
import FilterTable from "../FilterTable.jsx";

// Status constants
const USER_STATUS = {
  ACTIVE: 1,
  INACTIVE: 0,
};

const UserManagement = () => {
  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([]);
  const [activeTab, setActiveTab] = useState("userProfile");
  const [currentUserRole, setCurrentUserRole] = useState("User");
  const [selectedUserIndex, setSelectedUserIndex] = useState(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [showEditEmployeeModal, setShowEditEmployeeModal] = useState(false);
  const [editRole, setEditRole] = useState("");
  const [editStatus, setEditStatus] = useState(USER_STATUS.ACTIVE);
  const [isLoading, setIsLoading] = useState(true);
  const [loggedInUser, setLoggedInUser] = useState(null);

  const [activeFilter,setActiveFilter]=useState(null);
  const [filteredUsers,setFilteredUsers]=useState([])
  const [isFilterOpen,setIsFilterOpen]=useState(false)
  const [filters,setFilters]=useState(null)//keyword we're using for filtering

  const { COLORS } = useInitialColors();

  const loadData = async () => {
    try {
      setIsLoading(true);
      const { users, roles } = await fetchUsersAndRoles();
      const employees = await fetchAllEmployees();
      console.log(employees);
      setRoles(roles || []);
      const mappedUsers = (users || []).map((user) => {

        const employee = employees.find(e => e.email === user.email)
        console.log('local storage itself');
        console.log(localStorage)
        return {
          ...user,
          branch: `${employee.branch}` || "Uknown Branch",
          name: `${employee.name} ${employee.surname}` || user.email,
          role: user.role || roles.find((r) => Number(r.roleId) === Number(user.roleId))?.name || "Unknown Role",
          status: "Active",
          statusValue: USER_STATUS.ACTIVE,
        }
      });

      setUsers(mappedUsers);
      setFilteredUsers(mappedUsers);
      setLoggedInUser(mappedUsers[0] || null);
      setCurrentUserRole(getStoredUserRole().roleName || "User");

    } catch (error) {
      console.error("Failed to load data:", error);
      alert("Failed to load user data. Please try again.");
    } finally {
      setIsLoading(false);
    }
  };
  useEffect(() => {
      loadData();
    }, []);

    //Transform loaded data for a single layer of filter and searching
    const transformData=(loadedData,filter,search)=>{
        let result=[...loadedData]

        //Apply filters before searching 
        //returns all the keys of the object passed in 
            if(filter)
                result=result.filter(i=>i.branch===filter)
        
        
        //Search the filtered results. If no filter is applied Search should 
        //search the original loadedData
        if(search.trim() !== ""){
        const searchLower = searchTerm.toLowerCase();
        result=result.filter(item=>
            item.name.toLowerCase().includes(searchLower) ||
            item.email.toLowerCase().includes(searchLower)
            );
        }
        return result;
    }

    useEffect(()=>{
        const finalResult=transformData(users,filters,searchTerm);
        setFilteredUsers(finalResult);
    },[users,filters,searchTerm])

    
  const hasAdminRights = (role) => ["Admin", "SuperUser"].includes(role || "");

  const handleShowActions = (userIndex) => {
    const currentEmployee=localStorage.getItem("currentEmployee");
    const employee=JSON.parse(currentEmployee);
    setSelectedUserIndex(userIndex);

    const user = users[userIndex];
    if(user?.email===employee?.email)
    {
        setSelectedUserIndex(null)
        toast.error("You Cannot Change Your Own Role");
        return;
    }
  };

  const handleCloseActions = () => {
    setSelectedUserIndex(null);
  };

  const openEditEmployeeModal = () => {
    if (!hasAdminRights(currentUserRole)) {
      alert("Access denied: Admin rights required");
      return;
    }
    const user = users[selectedUserIndex];
    if (user) {
      setEditRole(user.roleId ?? "");
      setShowEditEmployeeModal(true);
      handleCloseActions();
    }
  };

  const saveEmployeeDetails = async () => {
    try {
      const user = users[selectedUserIndex];
      const normalizedEditRole = resolveRole(editRole);

      const selectedRole = roles.find(
        (r) => Number(r.roleId) === normalizedEditRole.roleId
      );

      await updateUserRole(user.userId, selectedRole.roleId);

      await updatedUsers(user.userId, {
        roleId: selectedRole.roleId,
        status: editStatus,
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
      });

      const updatedUsers = [...users];
      updatedUsers[selectedUserIndex] = {
        ...user,
        role: selectedRole.name,
        status: "Active",
        statusValue: USER_STATUS.ACTIVE,
        roleId: selectedRole.roleId,
      };
      setUsers(updatedUsers);
      setShowEditEmployeeModal(false);
      alert("User updated successfully!");
    } catch (error) {
      console.error("Update failed:", error);
      alert(`Failed to update user: ${error.message}`);
    }
  };

  const handleSaveUser = async (updatedData) => {
    try {
      const user = users[selectedUserIndex];
      if (!user) throw new Error("Invalid user");

      if (updatedData.roleId != null)
        await updateUserRole(user.userId, updatedData.roleId);

      const updatedUsers = [...users];
      updatedUsers[selectedUserIndex] = {
        ...user,
        ...updatedData,
        status:
          updatedData.statusValue !== undefined
            ? updatedData.statusValue === USER_STATUS.ACTIVE
              ? "Active"
              : "Inactive"
            : user.status,
      };
      setUsers(updatedUsers);
      return true;
    } catch (error) {
      console.error("Update failed:", error);
      alert(`Failed to update user: ${error.message}`);
      return false;
    }
  };

  const handleFilter=(val)=>{
    setFilters(val)
//   setActiveFilter(val)

//   if(!val){
//     setFilteredUsers(users)
//     return;
//   }

//   const filteredResult=users.filter(user=>user.branch===val)
//   setFilteredUsers(filteredResult)
  }

  const {
    activePage,
    setActivePage,
    itemsPerPage,
    setItemsPerPage,
    totalPages,
    indexOfFirstItem,
    indexOfLastItem,
    currentItems
  } = usePagination(users);

  const { dropdownOpen, toggleDropdown, closeDropdown } = useDropdown();

    const filterUsers = users.filter((user) => {
    const searchLower = searchTerm.toLowerCase();
    return (
      (user.name || "").toLowerCase().includes(searchLower) ||
      (user.email || "").toLowerCase().includes(searchLower) ||
      (user.role || "").toLowerCase().includes(searchLower)
    );
  });

  if (isLoading)
    return (
      <div className="menu-background">
        {/* Removed MenuBar here */}
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Loading user data...</p>
        </div>
      </div>
    );
  const handleItemsPerPageChange = (option) => {
    setItemsPerPage(option);
    closeDropdown();
    setActivePage(1);
  };


  return (
    <div className="menu-background">
      {/* Removed MenuBar here */}
      <div className="top-bar">
        <h2>User Management</h2>
        <button
          className="filter-btn"
          onClick={()=>setIsFilterOpen(prev=>!prev)}
        >
          <SlidersHorizontal size={20} />
          Filter
          <FilterTable
          data={users}
          filterKey="branch"
          onFilter={handleFilter}
          isOpen={isFilterOpen}
          onClose={()=>setIsFilterOpen(false)}
          />
        </button>
        <div className="search-bar" >
          <input
            type="text"
            placeholder=" Search users..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />

        <SearchIcon/>
        </div>
      </div>

          {/*Card on top of the table*/}
          <div className="payslip-card">
            <div className="payslip-ribbon">
              <span className="card-title">
                User Profile
              </span>
            </div>
            {/*2nd Row on is the rest of the table*/}
            <table className="styled-table">
              <thead>
                <tr className="heading">
                  <th>User</th>
                  <th>Email</th>
                  <th>Branch</th>
                  <th>Role</th>
                  <th className="action-col">Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredUsers.map((user, idx) => (
                  <tr key={idx}>
                    <td>
                      <div className="user-info">
                        <div className={`initials-circle
                        ${COLORS[idx % COLORS.length]}
                      `}>
                          {(`${(user.name[0] || "").charAt(0)}
                          ${(user.name[1] || "").charAt(0)}`)}</div>
                        <span className="user-name">{user.name || "Unknown User"}</span>
                      </div>
                    </td>
                    <td>{user.email || "No email"}</td>
                    <td>{user.branch}</td>
                    <td>
                      <span className={`role-badge ${(user.role || "").toLowerCase()}`}>
                        <FaUserLock /> {user.role || "Unknown Role"}
                      </span>
                    </td>


                    <td className="action-buttons">
                      <button
                        className="actions-trigger-btn"
                        onClick={() => handleShowActions(idx)} >
                        <FaEllipsisV />
                        Actions
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <RolesModal
            isOpen={selectedUserIndex !== null}
            onClose={handleCloseActions}
            user={selectedUserIndex !== null ? users[selectedUserIndex] : null}
            onSuccess={() => {
              loadData();
              handleCloseActions();
    toast.success("Employee Role Update Successful")
              // Optionally reload users if needed here
              loadData();
            }}
          />

          {showEditEmployeeModal && selectedUserIndex !== null && (
            <div
              className="actions-modal-overlay"
              onClick={() => setShowEditEmployeeModal(false)}>
              <div className="actions-modal" onClick={(e) => e.stopPropagation()}>
                <div className="actions-modal-header">
                  <h3>Edit {users[selectedUserIndex].name}</h3>
                  <button
                    className="close-btn"
                    onClick={() => setShowEditEmployeeModal(false)}
                  >
                    <FaTimes />
                  </button>
                </div>
                <div className="actions-modal-content">
                  <div className="form-group">
                    <label>Role</label>
                    <select
                      value={editRole}
                      onChange={(e) => setEditRole(Number(e.target.value))}
                      disabled={!hasAdminRights(currentUserRole)}
                    >
                      {/* Use roleId as value */}
                      {roles.map((role) => (
                        <option key={role.roleId} value={role.roleId}>
                          {role.name}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="form-group">
                    <label>Status</label>
                    <select
                      value={editStatus}
                      onChange={(e) => setEditStatus(Number(e.target.value))}
                      disabled={!hasAdminRights(currentUserRole)}
                    >
                      <option value={USER_STATUS.ACTIVE}>Active</option>
                      <option value={USER_STATUS.INACTIVE}>Inactive</option>
                    </select>
                  </div>
                  <div className="form-actions">
                    <button className="save-btn" onClick={saveEmployeeDetails}>
                      Save Changes
                    </button>
                    <button
                      className="cancel-btn"
                      onClick={() => setShowEditEmployeeModal(false)}
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              </div>
            </div>
          )}

          <div className="pagination-container">
            <div className="pagination-left-section">
              <span className="pagination-range">
                <strong className="range-bold">
                  {indexOfFirstItem + 1} -{" "}
                  {Math.min(indexOfLastItem, filteredUsers.length)}
                </strong>{" "}
                of {filteredUsers.length}
              </span>

              <div className="per-page-box" onClick={toggleDropdown}>
                <span className="per-page-number">{itemsPerPage}</span>
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Dropdown"
                  className="dropdown-icon"
                />
                {dropdownOpen && (
                  <div className="dropdown-options">
                    {[10].map((option) => (
                      <div
                        key={option}
                        className="dropdown-option"
                        onClick={() => handleItemsPerPageChange(option)}
                      >
                        {option}
                      </div>
                    ))}
                  </div>
                )}
              </div>

              <span className="per-page-label">Per page</span>
            </div>

            <div className="pagination-right-section">
              <div className="pagination-controls">
                {/* Go to First Page */}
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="First"
                  className={`pagination-arrow ${activePage === 1 ? "disabled" : ""}`}
                  onClick={() => activePage > 1 && setActivePage(1)}
                />

                {/* Go to Previous Page */}
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Previous"
                  className={`pagination-arrow ${activePage === 1 ? "disabled" : ""}`}
                  onClick={() => activePage > 1 && setActivePage(activePage - 1)}
                />

                {/* Page numbers remain the same */}
                <div className="page-count">
                  {Array.from({ length: totalPages || 1 }, (_, i) => {
                    const pageNum = i + 1;
                    return (
                      <button
                        key={pageNum}
                        onClick={() => setActivePage(pageNum)}
                        className={`page-number ${activePage === pageNum ? "active" : ""}`}
                      >
                        {pageNum}
                      </button>
                    );
                  })}
                </div>

                {/* Go to Next Page */}
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Next"
                  className={`pagination-arrow next ${activePage === totalPages ? "disabled" : ""
                    }`}
                  onClick={() =>
                    activePage < totalPages && setActivePage(activePage + 1)
                  }
                />

                {/* Go to Last Page */}
                <img
                  src="/images/arrow_drop_down_circle.png"
                  alt="Last"
                  className={`pagination-arrow next ${activePage === totalPages ? "disabled" : ""
                    }`}
                  onClick={() =>
                    activePage < totalPages && setActivePage(totalPages)
                  }
                />
              </div>
              <div className="employee-count">
                {`${filteredUsers.length} Admins @ Singular`}
              </div>
            </div>
          </div>
    </div>
  );
};

export default UserManagement;
