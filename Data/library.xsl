<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" indent="yes" encoding="utf-8" doctype-public="about:legacy-compat" />

  <xsl:template match="/">
    <html lang="uk">
      <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>Каталог публікацій кафедральної бібліотеки</title>
        <style>
          body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
            margin: 0;
            padding: 30px;
            background-color: #f4f6f9;
            color: #333333;
          }
          .container {
            max-width: 1100px;
            margin: 0 auto;
            background: #ffffff;
            padding: 35px;
            border-radius: 12px;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
          }
          h1 {
            color: #1a365d;
            margin-top: 0;
            font-size: 26px;
            border-bottom: 2px solid #e2e8f0;
            padding-bottom: 12px;
          }
          .subtitle {
            color: #718096;
            font-size: 14px;
            margin-bottom: 24px;
          }
          table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
          }
          th {
            background-color: #2b6cb0;
            color: #ffffff;
            font-weight: 600;
            text-align: left;
            padding: 12px 14px;
            font-size: 14px;
          }
          td {
            padding: 12px 14px;
            border-bottom: 1px solid #e2e8f0;
            font-size: 13.5px;
            vertical-align: top;
          }
          tr:hover {
            background-color: #f7fafc;
          }
          .badge {
            display: inline-block;
            padding: 3px 8px;
            border-radius: 6px;
            font-size: 12px;
            font-weight: 600;
            background-color: #edf2f7;
            color: #4a5568;
          }
          .badge-category {
            background-color: #ebf8ff;
            color: #2b6cb0;
          }
          .title {
            font-weight: 600;
            color: #2d3748;
          }
          .annotation {
            color: #718096;
            font-size: 12.5px;
            margin-top: 4px;
            line-height: 1.4;
          }
          .footer {
            margin-top: 30px;
            text-align: right;
            font-size: 12px;
            color: #a0aec0;
          }
        </style>
      </head>
      <body>
        <div class="container">
          <h1>Каталог публікацій кафедральної бібліотеки</h1>
          <div class="subtitle">
            Заклад: <xsl:value-of select="/Library/@university" /> | Сховище: <xsl:value-of select="/Library/@name" />
          </div>

          <table>
            <thead>
              <tr>
                <th style="width: 5%;">ID</th>
                <th style="width: 35%;">Назва та анотація</th>
                <th style="width: 18%;">Автор</th>
                <th style="width: 15%;">Категорія</th>
                <th style="width: 15%;">Факультет / Кафедра</th>
                <th style="width: 12%;">Рік</th>
              </tr>
            </thead>
            <tbody>
              <xsl:for-each select="/Library/Publication">
                <tr>
                  <td><strong><xsl:value-of select="@id" /></strong></td>
                  <td>
                    <div class="title"><xsl:value-of select="Title" /></div>
                    <div class="annotation"><xsl:value-of select="Annotation" /></div>
                  </td>
                  <td><xsl:value-of select="@author" /></td>
                  <td>
                    <span class="badge badge-category">
                      <xsl:value-of select="@category" />
                    </span>
                  </td>
                  <td>
                    <div><xsl:value-of select="@faculty" /></div>
                    <xsl:if test="@department">
                      <div class="badge" style="margin-top: 3px;">каф. <xsl:value-of select="@department" /></div>
                    </xsl:if>
                  </td>
                  <td>
                    <xsl:choose>
                      <xsl:when test="@year">
                        <xsl:value-of select="@year" />
                      </xsl:when>
                      <xsl:otherwise>—</xsl:otherwise>
                    </xsl:choose>
                  </td>
                </tr>
              </xsl:for-each>
            </tbody>
          </table>

          <div class="footer">
            Згенеровано системою кафедральної бібліотеки за допомогою XSLT
          </div>
        </div>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
