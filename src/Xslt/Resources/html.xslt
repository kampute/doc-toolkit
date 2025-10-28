<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:txt="http://kampute.com/doc-tools/transform/text"
  xmlns:doc="http://kampute.com/doc-tools/transform/xml-doc"
  exclude-result-prefixes="txt doc">

  <xsl:output method="html" indent="no"/>
  <xsl:strip-space elements="*"/>

  <!-- Template to match the root node and apply templates to its children -->
  <xsl:template match="/">
    <xsl:apply-templates/>
  </xsl:template>

  <!-- Template to handle text nodes -->
  <xsl:template match="text()">
    <xsl:value-of select="txt:NormalizeWhitespace(.)"/>
  </xsl:template>

  <!-- Template to handle 'para' elements, wrapping content in 'p' elements -->
  <xsl:template match="para">
    <p>
      <xsl:apply-templates/>
    </p>
  </xsl:template>

  <!-- Template to handle 'c' elements, wrapping content in 'code' elements -->
  <xsl:template match="c">
    <code>
      <xsl:apply-templates/>
    </code>
  </xsl:template>

  <!-- Template to handle 'code' elements, formatting content as code blocks with optional language class -->
  <xsl:template match="code">
    <xsl:variable xml:space="preserve" name="code" select="."/>
    <xsl:variable name="lang">
      <xsl:choose>
        <xsl:when test="@language">
          <xsl:value-of select="@language"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="doc:GetLanguageId($code)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <pre dir="ltr" aria-label="Code snippet">
      <code>
        <xsl:if test="string-length($lang) > 0">
          <xsl:attribute name="class">
            <xsl:value-of select="concat('language-', $lang)"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:value-of xml:space="preserve" select="doc:FormatCode($code)"/>
      </code>
    </pre>
  </xsl:template>

  <!-- Template to handle 'typeparamref' elements, wrapping content in 'code' elements with 'type-param-name' class -->
  <xsl:template match="typeparamref">
    <code class="type-param-name">
      <xsl:value-of select="@name"/>
    </code>
  </xsl:template>

  <!-- Template to handle 'paramref' elements, wrapping content in 'code' elements with 'param-name' class -->
  <xsl:template match="paramref">
    <code class="param-name">
      <xsl:value-of select="@name"/>
    </code>
  </xsl:template>

  <!-- Template to handle 'see' elements, formatting content as links based on 'langword', 'cref', or 'href' attributes -->
  <xsl:template match="see">
    <xsl:choose>
      <!-- Handle 'see' elements with 'langword' attribute -->
      <xsl:when test="@langword">
        <xsl:variable name="url" select="doc:GetKeywordUrl(@langword)"/>
        <xsl:choose>
          <xsl:when test="string-length($url) > 0">
            <a href="{$url}" rel="language-keyword" class="see-link">
              <code class="langword">
                <xsl:value-of select="@langword"/>
              </code>
            </a>
          </xsl:when>
          <xsl:otherwise>
            <code class="langword">
              <xsl:value-of select="@langword"/>
            </code>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- Handle 'see' elements with 'cref' attribute -->
      <xsl:when test="@cref">
        <xsl:variable name="url" select="doc:GetCodeReferenceUrl(@cref)"/>
        <xsl:variable name="text">
          <xsl:choose>
            <xsl:when test="string-length(text()) > 0">
              <xsl:value-of select="text()"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="title" select="doc:GetCodeReferenceTitle(@cref)"/>
              <xsl:choose>
                <xsl:when test="string-length($title) > 0">
                  <xsl:value-of select="$title"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="@cref"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="string-length($url) > 0">
            <a href="{$url}" rel="code-reference" class="see-link">
              <xsl:value-of select="$text"/>
            </a>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$text"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- Handle 'see' elements with 'href' attribute -->
      <xsl:when test="@href">
        <xsl:variable name="url" select="doc:GetTopicUrl(@href)"/>
        <xsl:variable name="text">
          <xsl:choose>
            <xsl:when test="string-length(text()) > 0">
              <xsl:value-of select="text()"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="title" select="doc:GetTopicTitle(@href)"/>
              <xsl:choose>
                <xsl:when test="string-length($title) > 0">
                  <xsl:value-of select="$title"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="@href"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <a href="{@href}" class="see-link">
          <xsl:choose>
            <xsl:when test="string-length($text) > 0">
              <xsl:value-of select="$text"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="@href"/>
            </xsl:otherwise>
          </xsl:choose>
        </a>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- Template to handle 'list' elements, formatting content as lists with optional type attribute -->
  <xsl:template match="list">
    <xsl:choose>
      <!-- Handle 'list' elements with 'type' attribute set to 'table' -->
      <xsl:when test="@type='table'">
        <table class="table-list">
          <xsl:for-each select="listheader">
            <thead>
              <tr>
                <xsl:for-each select="term|description">
                  <th>
                    <xsl:if test="name() = 'term'">
                      <xsl:attribute name="class">
                        <xsl:text>term</xsl:text>
                      </xsl:attribute>
                    </xsl:if>
                    <xsl:apply-templates/>
                  </th>
                </xsl:for-each>
              </tr>
            </thead>
          </xsl:for-each>
          <tbody>
            <xsl:for-each select="item">
              <tr>
                <xsl:for-each select="term|description">
                  <td>
                    <xsl:if test="name() = 'term'">
                      <xsl:attribute name="class">
                        <xsl:text>term</xsl:text>
                      </xsl:attribute>
                    </xsl:if>
                    <xsl:apply-templates/>
                  </td>
                </xsl:for-each>
              </tr>
            </xsl:for-each>
          </tbody>
        </table>
      </xsl:when>
      <!-- Handle 'list' elements with 'type' attribute set to 'number' -->
      <xsl:when test="@type='number'">
        <ol class="list-items">
          <xsl:call-template name="render-list-items"/>
        </ol>
      </xsl:when>
      <!-- Handle 'list' elements with not 'type' attribute or set to 'bullet' -->
      <xsl:otherwise>
        <ul class="list-items">
          <xsl:call-template name="render-list-items"/>
        </ul>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Helper template to render list items, handling 'listheader' and 'item' elements -->
  <xsl:template name="render-list-items">
    <xsl:for-each select="listheader|item">
      <li>
        <xsl:if test="term">
          <span class="term">
            <xsl:apply-templates select="term"/>
          </span>
          <span class="term-separator">
            <xsl:text xml:space="preserve"> &#8211; </xsl:text>
          </span>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="description">
            <xsl:apply-templates select="description"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates/>
          </xsl:otherwise>
        </xsl:choose>
      </li>
    </xsl:for-each>
  </xsl:template>

  <!-- Template to handle 'note' elements, formatting content as notes with optional type and title attributes -->
  <xsl:template match="note">
    <blockquote class="note" role="note">
      <xsl:if test="@type">
        <xsl:attribute name="data-type">
          <xsl:value-of select="@type"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@title">
        <xsl:attribute name="aria-label">
          <xsl:value-of select="@title"/>
        </xsl:attribute>
        <div class="note-title" aria-hidden="true">
          <xsl:value-of select="@title"/>
        </div>
      </xsl:if>
      <div class="note-content">
        <xsl:apply-templates/>
      </div>
    </blockquote>
  </xsl:template>

</xsl:stylesheet>
