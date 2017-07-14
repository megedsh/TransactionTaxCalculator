# TransactionTaxCalculator
Calculating financial transaction sum and tax (VAT)

This started as a project for lerning Unit Tests, then to lern the Stratagy Pattern, and finly lerning some git/github source control.

The code functionality is ver simple, 
Input:
- Enumerble transaction lines , which contains, Id, Tax rate, Tax code, Qtuantity, and line total.
- Global discounts (ammount or percentage, not both)

Output:
An object containing
- Transaction Totals (with and without tax)
- Transaction tax (VAT) split into groups and rates
- All totals are with and without the global discount.

Supports
 - Multiple tax groups and rates
 - Add or extract tax method
 - Global discount percent or global discount amount
 - Positive and negative quantity of items in the same transaction 
 
 
