export type Role = 0 | 1 | 2; // ajuste se no seu enum for diferente

export type Phone = {
  number: string;
};

export type Employee = {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  docNumber: string;
  birthDate: string; // "YYYY-MM-DD"
  role: Role;
  phones: Phone[];
  managerEmployeeId?: string | null;
};

export type EmployeeLookup = {
  id: string;
  name: string;
  email: string;
};

export type LoginRequest = { email: string; password: string };
export type LoginResponse = { accessToken: string };

export type CreateEmployeeRequest = {
  firstName: string;
  lastName: string;
  email: string;
  docNumber: string;
  birthDate: string;
  role: Role;
  phones: Phone[];
  password: string;
  managerEmployeeId?: string | null;
};

export type UpdateEmployeeRequest = {
  firstName: string;
  lastName: string;
  email: string;
  docNumber: string;
  birthDate: string;
  role: Role;
  phones: Phone[];
  managerEmployeeId?: string | null;
  password?: string | null;
};