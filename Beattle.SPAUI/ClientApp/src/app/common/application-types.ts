export type AuthorizationName =
  "View Users" | "Manage Users" |
  "View Roles" | "Manage Roles" | "Assign Roles";

export type AuthorizationValue =
  "users.view" | "users.manage" |
  "roles.view" | "roles.manage" | "roles.assign";

export type RoleOperation = "add" | "delete" | "update";

