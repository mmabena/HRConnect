import { jwtDecode } from "jwt-decode";

export const ROLE_IDS = Object.freeze({
  NormalUser: 0,
  SuperUser: 1,
});

const ROLE_NAMES_BY_ID = Object.freeze({
  [ROLE_IDS.NormalUser]: "NormalUser",
  [ROLE_IDS.SuperUser]: "SuperUser",
});

const normalizeRoleName = (value) => {
  if (typeof value !== "string") {
    return null;
  }

  const normalized = value.trim().toLowerCase();

  if (normalized === "0" || normalized === "normaluser") {
    return "NormalUser";
  }

  if (normalized === "1" || normalized === "superuser") {
    return "SuperUser";
  }

  return null;
};

export const resolveRole = (value) => {
  if (value == null) {
    return {
      roleId: null,
      roleName: null,
      key: null,
      isNormalUser: false,
      isSuperUser: false,
    };
  }

  let roleId = null;
  let roleName = null;

  if (typeof value === "object" && !Array.isArray(value)) {
    if (value.roleId != null && value.roleId !== "") {
      const parsedRoleId = Number(value.roleId);
      roleId = Number.isNaN(parsedRoleId) ? null : parsedRoleId;
    }

    roleName =
      normalizeRoleName(value.role) || normalizeRoleName(value.roleName);
  } else if (typeof value === "number") {
    roleId = Number.isNaN(value) ? null : value;
  } else if (typeof value === "string") {
    const parsedRoleId = Number(value);

    if (!Number.isNaN(parsedRoleId) && value.trim() !== "") {
      roleId = parsedRoleId;
    }

    roleName = normalizeRoleName(value);
  }

  if (roleName == null && roleId != null) {
    roleName = ROLE_NAMES_BY_ID[roleId] ?? null;
  }

  if (roleId == null && roleName != null) {
    roleId = roleName === "SuperUser" ? ROLE_IDS.SuperUser : ROLE_IDS.NormalUser;
  }

  return {
    roleId,
    roleName,
    key: roleName ? roleName.toLowerCase() : null,
    isNormalUser: roleId === ROLE_IDS.NormalUser,
    isSuperUser: roleId === ROLE_IDS.SuperUser,
  };
};

const parseStoredJson = (key) => {
  const rawValue = localStorage.getItem(key);

  if (!rawValue) {
    return null;
  }

  try {
    return JSON.parse(rawValue);
  } catch (error) {
    console.error(`Failed to parse ${key} from localStorage:`, error);
    return null;
  }
};

const getRoleFromToken = (token) => {
  if (!token) {
    return null;
  }

  try {
    const decoded = jwtDecode(token);
    return (
      decoded?.role ||
      decoded?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
      null
    );
  } catch (error) {
    console.error("Failed to decode token for role resolution:", error);
    return null;
  }
};

const hasResolvedRole = (resolvedRole) => resolvedRole?.roleId != null;

export const getStoredUserRole = () => {
  const currentUser = parseStoredJson("currentUser");
  const storedUser = parseStoredJson("user");
  const token = localStorage.getItem("token");

  const roleCandidates = [
    resolveRole(currentUser?.user),
    resolveRole(currentUser),
    resolveRole(storedUser),
    resolveRole(getRoleFromToken(token)),
  ];

  return roleCandidates.find(hasResolvedRole) ?? resolveRole(null);
};
